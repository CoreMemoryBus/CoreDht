using CoreMemoryBus.Messaging;

namespace CoreDht.Node
{
    public interface IMessageBus : IPublisher, ISubscriber
    { }
}