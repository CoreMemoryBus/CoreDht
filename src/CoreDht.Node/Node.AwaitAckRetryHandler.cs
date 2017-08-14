using System;
using System.Collections.Generic;
using System.Threading;
using CoreDht.Node.Messages;
using CoreDht.Node.Messages.Internal;
using CoreDht.Utils;
using CoreMemoryBus;

namespace CoreDht.Node
{
    partial class Node
    {
        public class AwaitAckRetryHandler : IDisposable
            , IHandle<AwaitMessage>
            , IHandle<AwaitWithTimeoutMessage>
            , IHandle<AckMessage>
            , IHandle<ScheduleRetryAction>
        {
            private readonly IActionScheduler _actionScheduler;
            private readonly IExpiryTimeCalculator _expiryCalculator;
            private readonly ICommunicationManager _commMgr;
            private readonly Action<string> _logger;
            private readonly NodeConfiguration _config;
            private readonly Dictionary<CorrelationId, DateTime> _acks = new Dictionary<CorrelationId, DateTime>();

            public AwaitAckRetryHandler(IActionScheduler actionScheduler, IExpiryTimeCalculator expiryCalculator, NodeHandlerContext handlerContext)
            {
                _actionScheduler = actionScheduler;
                _disposeAction = new DisposableAction(
                    () => { _actionScheduler.ExecuteAction += OneExecuteAction; },
                    () => { _actionScheduler.ExecuteAction -= OneExecuteAction; });

                _expiryCalculator = expiryCalculator;
                _commMgr = handlerContext.CommunicationManager;
                _config = handlerContext.Configuration;
                _logger = handlerContext.Logger;
            }

            private void OneExecuteAction(object sender, ActionSchedulerEventArgs e)
            {
                // Decide if we should execute the action or reschedule to a new time.
                var context = e.State as Context;
                if (context != null)
                {
                    var correlation = context.CorrelationId;

                    DateTime newDueTime;
                    if (_acks.TryGetValue(correlation, out newDueTime))
                    {
                        e.RescheduleAt = newDueTime;
                        _acks.Remove(correlation);
                    }
                }
            }

            private class Context
            {
                public CorrelationId CorrelationId { get; set; }
            }

            public void Handle(AwaitMessage message)
            {
                var correlation = message.CorrelationId;
                var dueTime = _expiryCalculator.CalcExpiry(_config.AwaitTimeout);
                // If our await expires, we are no longer interested in any further action or timeout extensions.
                _actionScheduler.ScheduleAction(dueTime, new Context { CorrelationId = correlation },
                cxt =>
                {
                    _commMgr.SendInternal(new CancelOperation(correlation));
                    _acks.Remove(correlation);
                });

                _logger?.Invoke($"{message.GetType().Name} Id:{message.CorrelationId} ({_config.AwaitTimeout} ms)");
            }

            public void Handle(AwaitWithTimeoutMessage message)
            {
                var correlation = message.CorrelationId;
                var dueTime = _expiryCalculator.CalcExpiry(message.Timeout);

                _actionScheduler.ScheduleAction(dueTime, new Context { CorrelationId = correlation },
                cxt =>
                {
                    _commMgr.SendInternal(new CancelOperation(correlation));
                    _acks.Remove(correlation);
                });

                _logger?.Invoke($"{message.GetType().Name} Id:{message.CorrelationId} ({message.Timeout} ms)");

            }

            public void Handle(AckMessage message)
            {
                // Extend a timout while the sender of the Ack is processing the original request. 
                _logger?.Invoke($"AckMessage received Id:{message.CorrelationId}");

                var dueTime = _expiryCalculator.CalcExpiry(_config.AckTimeout);
                _acks[message.CorrelationId] = dueTime;
            }

            public void Handle(ScheduleRetryAction message)
            {
                var correlation = message.CorrelationId;
                if (_config.RetryTimeout != Timeout.Infinite)
                {
                    var expiryTime = _expiryCalculator.CalcExpiry(_config.RetryTimeout);
                    _actionScheduler.ScheduleAction(expiryTime, new Context { CorrelationId = correlation },
                    cxt =>
                    {
                        _commMgr.SendInternal(new RetryAction(correlation));
                    });
                }
            }

            #region IDisposable Support

            private bool _isDisposed = false;
            private readonly DisposableAction _disposeAction;

            public void Dispose()
            {
                if (!_isDisposed)
                {
                    _disposeAction.Dispose();
                    _isDisposed = true;
                }
            }

            #endregion
        }
    }
}