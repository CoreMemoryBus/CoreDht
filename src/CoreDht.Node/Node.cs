using System;
using NetMQ;
using NetMQ.Sockets;

namespace CoreDht.Node
{
    public class Node : IDisposable
    {
        public NodeConfiguration Configuration { get; }
        protected NodeServices Services { get; set; }
        protected Action<string> Logger { get; }
        protected DealerSocket ListeningSocket { get; }
        protected NodeActor Actor { get; }

        public NodeInfo Identity { get; private set; }
        public NodeInfo Predecessor { get; private set; }
        public NodeInfo Successor { get; protected set; }
        // SuccessorTable
        // ChordTable


        protected Node(string hostAndPort, string identifier, NodeConfiguration configuration, NodeServices services)
        {
            Configuration = configuration;
            Services = services;
            Logger = Services.Logger;
            Identity = new NodeInfo(identifier, Services.ConsistentHashingService.GetConsistentHash(identifier), hostAndPort);
            Predecessor = Identity;
            Successor = Identity;
            ListeningSocket = Services.SocketFactory.CreateBindingSocket(hostAndPort);
            Actor = new NodeActor(ListeningSocket, OnReceiveMsg, (socket, ex) => {});
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
            var typeCode = msg[0].ConvertToString();
            switch (typeCode)
            {
                case NetMQActor.EndShimMessage:
                    Stop();
                    Logger?.Invoke($"Node terminating.");
                    break;
            }
        }

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