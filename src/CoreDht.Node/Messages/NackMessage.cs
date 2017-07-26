using CoreDht.Utils;
using CoreMemoryBus.Messages;

namespace CoreDht.Node.Messages
{
    /// <summary>
    /// A NackMessage is sent as a reply when a node has successfully received a message, but cannot act upon it owing to it's present state.
    /// This is typically when the node is involved in network maintenance operations as a priority and cannot process application messages at the time.
    /// </summary>
    public class NackMessage : Message, ICorrelatedMessage<CorrelationId>
    {
        public NackMessage(CorrelationId correlationId)
        {
            CorrelationId = correlationId;
        }

        public CorrelationId CorrelationId { get; }
    }
}