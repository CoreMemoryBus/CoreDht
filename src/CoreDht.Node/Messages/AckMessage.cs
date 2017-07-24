using CoreDht.Utils;
using CoreMemoryBus.Messages;

namespace CoreDht.Node.Messages
{
    /// <summary>
    /// An AckMessage (and derived messages) is sent in response to a time sensitive message. 
    /// On receipt, it will extend the timeout and help in verifying the health of a connection.
    /// </summary>
    public class AckMessage : Message, ICorrelatedMessage<CorrelationId>
    {
        public AckMessage(CorrelationId correlationId)
        {
            CorrelationId = correlationId;
        }

        public CorrelationId CorrelationId { get; }
    }
}