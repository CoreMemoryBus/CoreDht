using System;
using CoreDht.Node;
using NetMQ;
using NetMQ.Sockets;

namespace SimpleCircularNetwork
{
    public class SimpleNode : IDisposable
    {
        private readonly DealerSocket _listeningSocket;
        private readonly Action<string> _logger;
        private readonly NodeActor _actor;

        public NodeInfo Identity { get; private set; }
        public NodeInfo Successor { get; set; }

        public SimpleNode(NodeInfo identity, DealerSocket listeningSocket, Action<string> logger)
        {
            Identity = Successor = identity;
            _listeningSocket = listeningSocket;
            _logger = logger;
            _actor = new NodeActor(_listeningSocket, MessageHandler);
        }

        private void MessageHandler(NetMQMessage obj)
        {
            
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            if (disposedValue) return;

            _actor.Dispose();
            _listeningSocket.Dispose();

            disposedValue = true;
        }
        #endregion
    }
}