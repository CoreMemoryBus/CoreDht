using System;
using CoreDht.Utils;
using CoreDht.Utils.Hashing;
using CoreMemoryBus.Messaging;
using NetMQ;
using NetMQ.Sockets;

namespace CoreDht.Node
{
    public class Node : IDisposable, IChordNode
    {
        public NodeConfiguration Configuration { get; }
        protected NodeServices Services { get; set; }
        protected IUtcClock Clock { get; }
        protected Action<string> Logger { get; }
        protected DealerSocket ListeningSocket { get; }
        protected NodeActor Actor { get; }
        public NodeInfo Identity { get; private set; }
        public NodeInfo Predecessor { get; private set; }
        protected MemoryBus MessageBus { get; }
        private SocketCache ForwardingSockets { get; }
        protected ICommunicationManager CommunicationManager { get; }


        public NodeInfo Successor
        {
            get { return Successors[0].SuccessorIdentity; }
            protected set { Successors[0] = new RoutingTableEntry(value.RoutingHash, value); }
        }
        protected SuccessorTable Successors { get; }
        protected ChordRoutingTable RoutingTable { get; }

        protected Node(string hostAndPort, string identifier, NodeConfiguration configuration, NodeServices services)
        {
            Configuration = configuration;
            Services = services;
            Logger = Services.Logger;
            Clock = services.Clock;
            Identity = new NodeInfo(identifier, Services.ConsistentHashingService.GetConsistentHash(identifier), hostAndPort);
            Predecessor = Identity;
            Successors = new SuccessorTable(Identity, configuration.SuccessorCount);
            RoutingTable = new ChordRoutingTable(Identity, Identity.RoutingHash.BitCount);
            Successor = Identity;
            ListeningSocket = Services.SocketFactory.CreateBindingSocket(hostAndPort);
            Actor = new NodeActor(ListeningSocket, OnReceiveMsg, (socket, ex) => {});
            ForwardingSockets = new SocketCache(services.SocketFactory, Clock);
            MessageBus = new MemoryBus();
            CommunicationManager = Services.CommunicationManagerFactory.Create(this, ForwardingSockets, MessageBus);
        }

        public void Start()
        {
            Actor.Start();
        }

        public void Stop()
        {
            Actor.Stop();
        }

        protected Node(string hostAndPort, string identifier, NodeServices services) 
            : this(hostAndPort, identifier, new DefaultNodeConfiguration(), services)
        { }

        void OnReceiveMsg(NetMQMessage msg)
        {
            CommunicationManager.Receive(msg);
        }

        #region IChordNode

        NodeInfo IChordNode.Identity => Identity;

        NodeInfo IChordNode.Successor => Successor;

        NodeInfo IChordNode.FindClosestPrecedingNode(ConsistentHash startingHash)
        {
            return RoutingTable.FindClosestPrecedingNode(startingHash);
        }

        void IChordNode.Stop() { Actor.Stop(); }

        #endregion

        #region IDisposable Support

        private bool _isDisposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (Actor.IsRunning)
                    {
                        Stop();
                    }
                    Actor.Dispose();
                    ListeningSocket.Dispose();
                }

                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }


        #endregion
    }
}