using CoreMemoryBus.Messaging;

namespace CoreDht.Node
{
    public class CommunicationManagerFactory : ICommunicationManagerFactory
    {
        private readonly INodeMarshallerFactory _marshallerFactory;

        public CommunicationManagerFactory(INodeMarshallerFactory marshallerFactory)
        {
            _marshallerFactory = marshallerFactory;
        }

        public ICommunicationManager Create(IChordNode node, ISocketCache socketCache, IPublisher publisher)
        {
            return new CommunicationManager(node, _marshallerFactory.Create(), socketCache, publisher);
        }
    }
}