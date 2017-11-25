using CoreDht.Utils;
using CoreMemoryBus.Messages;

namespace CoreDht.Node.Messages
{
    public interface ICorrelatedNodeMessage : ICorrelatedMessage<CorrelationId>
    { }
}