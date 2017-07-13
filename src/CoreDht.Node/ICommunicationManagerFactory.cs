using CoreMemoryBus.Messaging;

namespace CoreDht.Node
{
    public interface ICommunicationManagerFactory
    {
        ICommunicationManager Create(IChordNode node, ISocketCache socketCache, IPublisher publisher);
    }
}