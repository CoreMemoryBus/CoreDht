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
    /// If an AckMessage is received for a retryable operation, no further retries will be attempted.
    /// Reminder: the number of actual attempts is 1 + retry count.
    /// </summary>
    public class RetryOperationHandler
        : IHandle<NackMessage>
        , IHandle<AckMessage>
    {
        private readonly IMessageBus _messageBus;
        private readonly CorrelationId _correlationId;
        private readonly Action _action;
        private readonly int _retryMax;
        private int _retryCount;

        public RetryOperationHandler(IMessageBus messageBus, CorrelationId correlationId, Action action, int retryCount)
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
                ScheduleRetry();
            }
            else if (_retryMax == RetryCount.Infinite)
            {
                _action();
                ScheduleRetry();
            }
            else
            {
                Terminate();
            }
        }

        private void ScheduleRetry()
        {
            _messageBus.Publish(new ScheduleRetryAction(_correlationId));
        }

        public void Terminate()
        {
            _messageBus.Unsubscribe(this);
        }

        public void Handle(NackMessage message)
        {
            if (message.CorrelationId.Equals(_correlationId))
            {
                Invoke();
            }
        }

        public void Handle(AckMessage message)
        {
            if (message.CorrelationId.Equals(_correlationId))
            {
                Terminate();
            }
        }
    }
}