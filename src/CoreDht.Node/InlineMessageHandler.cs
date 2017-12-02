using System;
using CoreDht.Utils;
using CoreMemoryBus;
using CoreMemoryBus.Messages;
using CoreMemoryBus.Handlers;
using CoreDht.Node.Messages;

namespace CoreDht.Node
{
    /// <summary>
    /// InlineMessageHandler is a simple inline, typesafe message handler which invokes an action in response to a correlated message.
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    public class InlineMessageHandler<TMessage>
        : IHandle<TMessage>
        where TMessage : ICorrelatedNodeMessage
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
            if (message is ICorrelatedNodeMessage correlatedMessage && 
                correlatedMessage.CorrelationId.Equals(_correlationId))
            {
                _response();
            }
        }
    }
}