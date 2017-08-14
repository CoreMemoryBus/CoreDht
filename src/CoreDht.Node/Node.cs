using System;
using CoreDht.Node.Messages.Internal;
using CoreDht.Utils;
using CoreDht.Utils.Hashing;
using CoreMemoryBus.Messaging;
using NetMQ;
using NetMQ.Sockets;

namespace CoreDht.Node
{
    public partial class Node : IDisposable, IChordNode
    {
        public NodeConfiguration Configuration { get; }
        protected NodeServices Services { get; set; }
        protected DisposableStack Janitor { get; }
        protected IUtcClock Clock { get; }
        protected Action<string> Logger { get; }
        protected DealerSocket ListeningSocket { get; }
        protected NodeActor Actor { get; }
        public NodeInfo Identity { get; private set; }
        public NodeInfo Predecessor { get; private set; }
        protected MemoryBus MessageBus { get; }
        private SocketCache ForwardingSockets { get; }
        protected ICommunicationManager CommunicationManager { get; }
        protected SuccessorTable Successors { get; }
        protected ChordRoutingTable RoutingTable { get; }
        protected NodeActionScheduler Scheduler { get; }

        protected Node(string hostAndPort, string identifier, NodeConfiguration configuration, NodeServices services)
        {
            Configuration = configuration;
            Services = services;
            Janitor = new DisposableStack();
            Logger = Services.Logger;
            Clock = services.Clock;
            Identity = new NodeInfo(identifier, Services.ConsistentHashingService.GetConsistentHash(identifier), hostAndPort);
            Predecessor = Identity;
            Successors = new SuccessorTable(Identity, configuration.SuccessorCount);
            RoutingTable = new ChordRoutingTable(Identity, Identity.RoutingHash.BitCount);
            Successor = Identity;
            ListeningSocket = Janitor.Push(Services.SocketFactory.CreateBindingSocket(hostAndPort));
            Actor = Janitor.Push(new NodeActor(ListeningSocket, OnReceiveMsg, (socket, ex) => {}));
            ForwardingSockets = Janitor.Push(new SocketCache(services.SocketFactory, Clock));
            MessageBus = new MemoryBus();
            CommunicationManager = Services.CommunicationManagerFactory.Create(this, ForwardingSockets, MessageBus);
            Scheduler = Janitor.Push(new NodeActionScheduler(Clock, services.TimerFactory.CreateTimer(), CommunicationManager));
            var timerHandler = Janitor.Push(Scheduler.CreateTimerHandler());
            MessageBus.Subscribe(timerHandler);
            Janitor.Push(new DisposableAction(() => MessageBus.Unsubscribe(timerHandler)));
            MessageBus.Subscribe(new LifetimeHandler(this));
            var handlercontext = CreateHandlerContext();
            MessageBus.Subscribe(Janitor.Push(new AwaitAckRetryHandler(Scheduler, Services.ExpiryTimeCalculator, handlercontext)));
            MessageBus.Subscribe(new JoinNetworkHandler(handlercontext));
        }

        protected Node(string hostAndPort, string identifier, NodeServices services)
            : this(hostAndPort, identifier, new DefaultNodeConfiguration(), services)
        { }

        protected NodeHandlerContext CreateHandlerContext()
        {
            return new NodeHandlerContext
            {
                Logger = Logger,
                Configuration = Configuration,
                Identity = Identity,
                CommunicationManager = CommunicationManager,
                Scheduler = Scheduler,
                ExpiryTimeCalculator = Services.ExpiryTimeCalculator,
                CorrelationIdFactory = Services.CorrelationIdFactory,
                MessageBus = MessageBus,
            };
        }

        public NodeInfo Successor
        {
            get { return Successors[0].SuccessorIdentity; }
            protected set { Successors[0] = new RoutingTableEntry(value.RoutingHash, value); }
        }

        public void Start()
        {
            Initialize();
            Actor.Start();
            CommunicationManager.SendInternal(new NodeInitialised());
        }

        protected virtual void Initialize()
        {
            // Do stuff like calculate a seed node to join
            // Todo: Create a list of potential known seed nodes using a multicast beacon
        }

        protected virtual void OnInitialised()
        {
            if (!IsSeedNode())
            {
                CommunicationManager.SendInternal(new BeginJoinNetwork(Configuration.SeedNodeIdentity));
            }
        }

        private bool IsSeedNode()
        {
            return Identity.Identifier.Equals(Configuration.SeedNodeIdentity);
        }

        public void Stop()
        {
            Actor.Stop();
        }

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
                    Janitor.Dispose();
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