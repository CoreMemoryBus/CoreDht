using System;
using CoreDht.Node;
using CoreDht.Utils.Hashing;

namespace SimpleCircularNetwork
{
    public class SimpleNodeFactory
    {
        private readonly IConsistentHashingService _hashingService;
        private readonly InProcNodeSocketFactory _socketFactory;

        public SimpleNodeFactory(IConsistentHashingService hashingService)
        {
            _hashingService = hashingService;
            _socketFactory = new InProcNodeSocketFactory();
        }

        public SimpleNode CreateNode(string identifier)
        {
            var routingHash = _hashingService.GetConsistentHash(identifier);
            var identity = new NodeInfo(identifier, routingHash, identifier);
            var hostAndPort = identifier;
            return new SimpleNode(identity, _socketFactory.CreateBindingSocket(hostAndPort), Console.WriteLine);
        }
    }
}