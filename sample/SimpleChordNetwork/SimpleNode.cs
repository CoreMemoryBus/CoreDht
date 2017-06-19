using System;
using CoreDht.Node;
using CoreDht.Utils;
using CoreDht.Utils.Hashing;
using CoreMemoryBus;
using CoreMemoryBus.Messages;
using CoreMemoryBus.Messaging;
using NetMQ;
using NetMQ.Sockets;
using SimpleChordNetwork.Messages;

namespace SimpleChordNetwork
{
    public class SimpleNode 
        : IHandle<Terminate>
        , IHandle<DisplayDomain>
        , IDisposable
    {
        private readonly DealerSocket _listeningSocket;
        private readonly Action<string> _logger;
        private readonly NodeActor _actor;
        private readonly MessageSerializer _serializer;
        private readonly MemoryBus _messageBus;

        private readonly AnimalRepository _repository;

        public NodeInfo Identity { get; private set; }
        public NodeInfo Successor { get; set; }

        public SimpleNode(NodeInfo identity, DealerSocket listeningSocket, Action<string> logger)
        {
            Identity = Successor = identity;
            _listeningSocket = listeningSocket;
            _logger = logger;
            _serializer = new MessageSerializer();
            _messageBus = new MemoryBus();
            _repository = new AnimalRepository(Log);
            _messageBus.Subscribe(this);
            _messageBus.Subscribe(_repository);
            _actor = new NodeActor(_listeningSocket, MessageHandler);
        }

        const int RoutingHashIndex = 0;
        const int PayloadFrameIndex = 1;

        private DealerSocket _forwardingSocket;

        private DealerSocket CreateForwardingSocket()
        {
            return new DealerSocket($">inproc://{Successor.HostAndPort}");
        }

        DealerSocket ForwardingSocket => _forwardingSocket ?? (_forwardingSocket = CreateForwardingSocket());

        private void MessageHandler(NetMQMessage mqMsg)
        {
            var routingHash = new ConsistentHash(mqMsg[RoutingHashIndex].Buffer);
            //Log($"Received msg for {(int)routingHash.Bytes[0]}");
            if (routingHash.IsBetween(Identity.RoutingHash, Successor.RoutingHash))
            {
                //Log($"Accepting");
                UnmarshallMsg(mqMsg);
            }
            else // forward to successor
            {
                //Log($"Forwarding");
                ForwardingSocket.SendMultipartMessage(mqMsg);
            }
        }

        void Log(string logMessage)
        {
            _logger?.Invoke($"{Identity.Identifier}\t{DateTime.Now.ToString("hh:mm:ss.fff")}\t{logMessage}");
        }
        
        public NetMQMessage MarshallMsg(ConsistentHash destinationHash, Message msg)
        {
            var json = _serializer.Serialize(msg);
            var mqMsg = new NetMQMessage(new []
            {
               new NetMQFrame(destinationHash.Bytes),
               new NetMQFrame(json), 
            });

            return mqMsg;
        }

        private void UnmarshallMsg(NetMQMessage mqMsg)
        {
            var msg = _serializer.Deserialize<Message>(json: mqMsg[PayloadFrameIndex].ConvertToString());
            _messageBus.Publish(msg);
        }
        
        public void SendToNode(Message msg)
        {
            var mqMsg = MarshallMsg(Identity.RoutingHash, msg);
            _actor.SendMultipartMessage(mqMsg);
        }

        public void SendToNetwork(Message msg)
        {
            var routedMsg = msg as IRoutableMessage;
            if (routedMsg != null)
            {
                var mqMsg = MarshallMsg(routedMsg.RoutingTarget, msg);
                _actor.SendMultipartMessage(mqMsg);
            }
        }

        #region Handlers

        public void Handle(DisplayDomain message)
        {
            Log($"Range:[{(int) Identity.RoutingHash.Bytes[0]},{Successor.RoutingHash.Bytes[0]})");
        }

        public void Handle(Terminate message)
        {
            _actor.Stop();
            // and forward to successor
        }

        #endregion

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