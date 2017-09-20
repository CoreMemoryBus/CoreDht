using System;
using CoreDht.Node.Messages;
using CoreDht.Node.Messages.Internal;
using CoreDht.Utils;
using CoreMemoryBus;
using CoreMemoryBus.Messaging;
using CoreMemoryBus.Handlers;

namespace CoreDht.Node
{
    /// <summary>
    /// RetryOperationHandler manages operations that may need to be re-attempted where a failed operation is a likely outcome.
    /// RetryOperationHandler maintains an internal retry count and interacts with the action scheduler to perform the supplied
    /// action at some future time. A NackMessage is employed exclusively to indicate that an operation should be retried at some future time.
    /// RetryOperationHandler will handle the NackMessage and schedule a retry action.
    /// Reminder: the number of actual attempts is 1 + retry count.
    /// </summary>
    public class RetryOperationHandler
        : IHandle<NackMessage>
    {
        private readonly MemoryBus _messageBus;
        private readonly CorrelationId _correlationId;
        private readonly Action _action;
        private readonly int _retryMax;
        private int _retryCount;

        public RetryOperationHandler(MemoryBus messageBus, CorrelationId correlationId, Action action, int retryCount)
        {
            _messageBus = messageBus;
            _correlationId = correlationId;
            _action = action;
            _retryMax = retryCount == RetryCount.Infinite ? RetryCount.Infinite : Math.Max(0, retryCount);
        }

        public void Invoke()
        {
            if (_retryCount <= _retryMax)
            {
                _action();
                ++_retryCount;
                _messageBus.Publish(new ScheduleRetryAction(_correlationId));
            }
            else if (_retryMax == RetryCount.Infinite)
            {
                _action();
                _messageBus.Publish(new ScheduleRetryAction(_correlationId));
            }
            else
            {
                _messageBus.Unsubscribe(this);
            }
        }

        public void Handle(NackMessage message)
        {
            if (message.CorrelationId.Equals(_correlationId))
            {
                Invoke();
            }
        }
    }
}