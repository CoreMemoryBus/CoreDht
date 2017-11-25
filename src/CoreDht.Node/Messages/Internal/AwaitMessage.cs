using CoreDht.Utils;
using CoreMemoryBus.Messages;

namespace CoreDht.Node.Messages.Internal
{
    public class AwaitMessage : Message, ICorrelatedNodeMessage
    {
        public AwaitMessage(CorrelationId correlationId)
        {
            CorrelationId = correlationId;
        }

        public CorrelationId CorrelationId { get; }
    }
}