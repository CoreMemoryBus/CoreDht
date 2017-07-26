using System;
using CoreDht.Node.Messages;
using CoreDht.Node.Messages.Internal;
using CoreDht.Utils;
using CoreMemoryBus.Messaging;

namespace CoreDht.Node
{
    public class RetryOperation
    {
        private readonly IPublisher _publisher;
        private readonly CorrelationId _correlationId;
        private readonly Action _action;
        private int _retryCount;

        public RetryOperation(IPublisher publisher, CorrelationId correlationId, Action action, int retryCount)
        {
            _publisher = publisher;
            _correlationId = correlationId;
            _action = action;
            _retryCount = retryCount;
        }

        public void Invoke()
        {
            if (_retryCount >= 0)
            {
                _action();
                _publisher.Publish(new ScheduleRetryAction(_correlationId));
            }
            --_retryCount;
        }
    }
}