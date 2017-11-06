using CoreMemoryBus.Messaging;

namespace CoreDht.Node
{
    public interface IMessageBusFactory
    {
        IMessageBus Create();
    }
}