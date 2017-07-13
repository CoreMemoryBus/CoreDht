using System;
using CoreDht.Node;
using CoreDht.Utils;
using CoreDht.Utils.Hashing;

namespace CommunicationManager
{
    public class NodeServicesFactory
    {
        private readonly IConsistentHashingService _hashingService;

        public NodeServicesFactory(IConsistentHashingService hashingService)
        {
            _hashingService = hashingService;
        }

        public NodeServices CreateServices()
        {
            var result = new DefaultInprocNodeServices
            {
                Logger = Console.WriteLine,
                ConsistentHashingService = _hashingService,
                CommunicationManagerFactory = new CommunicationManagerFactory(new NodeMarshallerFactory(new MessageSerializer()))
            };
            return result;
        }
    }
}