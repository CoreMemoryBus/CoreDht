using CoreMemoryBus.Messaging;

namespace CoreDht.Node
{
    public class MessageBusFactory : IMessageBusFactory
    {
        public IMessageBus Create()
        {
            return new MemoryBus();
        }
    }
}