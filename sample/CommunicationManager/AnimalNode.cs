﻿using CoreDht.Node;
using CoreDht.Utils.Messages;
using SimpleNetworkCommon;

namespace CommunicationManager
{
    public class AnimalNode : Node
    {
        private readonly AnimalRepository _repository;

        public AnimalNode(string hostAndPort, string identifier, NodeServices services)
            : base(hostAndPort, identifier, services)
        {
            // We'll suppress the autojoin behaviour for the demo as we're building the network manually
            Configuration.SeedNodeIdentity = Identity;

            _repository = new AnimalRepository(services.Logger);
            MessageBus.Subscribe(_repository);
        }

        public new NodeInfo Successor
        {
            get { return base.Successor; }
            set { base.Successor = value; }
        }

        public new ChordRoutingTable RoutingTable => base.RoutingTable;

        public void SendToNetwork(RoutableMessage msg)
        {
            CommunicationManager.Send(msg);
        }
    }
}