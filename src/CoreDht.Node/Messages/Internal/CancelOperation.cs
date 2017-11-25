using CoreDht.Utils;
using CoreMemoryBus.Messages;

namespace CoreDht.Node.Messages.Internal
{
    public class CancelOperation : Message, ICorrelatedNodeMessage
    {
        public CancelOperation(CorrelationId correlationId)
        {
            CorrelationId = correlationId;
        }

        public CorrelationId CorrelationId { get; }
    }
}