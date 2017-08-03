using System;
using CoreDht.Utils;
using CoreMemoryBus;
using CoreMemoryBus.Messages;

namespace CoreDht.Node
{
    /// <summary>
    /// InlineMessageHandler is a simple inline, typesafe message handler which invokes an action in response to a correlated message.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public class InlineMessageHandler<TMessage>
        : IHandle<TMessage>
        where TMessage : ICorrelatedMessage<CorrelationId>
    {
        private readonly CorrelationId _correlationId;
        private readonly Action _response;

        public InlineMessageHandler(CorrelationId correlationId, Action response)
        {
            _correlationId = correlationId;
            _response = response;
        }

        public void Handle(TMessage message)
        {
            var correlatedMessage = message as ICorrelatedMessage<CorrelationId>;
            if (correlatedMessage != null && correlatedMessage.CorrelationId.Equals(_correlationId))
            {
                _response();
            }
        }
    }
}