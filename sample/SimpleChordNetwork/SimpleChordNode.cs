using System;
using CoreDht.Node;
using CoreDht.Utils;
using CoreDht.Utils.Hashing;
using CoreDht.Utils.Messages;
using CoreMemoryBus;
using CoreMemoryBus.Messages;
using CoreMemoryBus.Messaging;
using NetMQ;
using NetMQ.Sockets;
using SimpleNetworkCommon;
using SimpleNetworkCommon.Messages;
using CoreMemoryBus.Handlers;

namespace SimpleChordNetwork
{
    public class SimpleChordNode 
        : IHandle<TerminateDht>
        , IDisposable
    {
        private readonly IUtcClock _clock;
        private readonly DealerSocket _listeningSocket;
        private readonly Action<string> _logger;
        private readonly NodeActor _actor;
        private readonly MessageSerializer _serializer;
        private readonly MemoryBus _messageBus;
        private readonly SocketCache _socketCache;
        private readonly INodeSocketFactory _socketFactory;
        private readonly ChordRoutingTable _routingTable;

        private readonly AnimalRepository _repository;

        public NodeInfo Identity { get; private set; }
        public NodeInfo Successor { get; set; }

        public SimpleChordNode(NodeInfo identity, DealerSocket listeningSocket, Action<string> logger)
        {
            Identity = Successor = identity;
            _clock = new UtcClock();
            _listeningSocket = listeningSocket;
            _logger = logger;
            _serializer = new MessageSerializer();
            _messageBus = new MemoryBus();
            _messageBus.Subscribe(this);
            _repository = new AnimalRepository(Log);
            _messageBus.Subscribe(_repository);
            _actor = new NodeActor(_listeningSocket, MessageHandler);
            _socketFactory = new InProcNodeSocketFactory();
            _socketCache = new SocketCache(_socketFactory, _clock);
            _socketCache.AddActor(Identity.HostAndPort, _actor);
            _routingTable = new ChordRoutingTable(Identity, Identity.RoutingHash.BitCount);
            _actor.Start();
        }

        public ChordRoutingTable RoutingTable => _routingTable;

        const int RoutingHashFrameIndex = 0;
        const int HopCountFrameIndex = 1;
        const int RoutingTechniqueIndex = 2;
        const int PayloadFrameIndex = 3;

        private void MessageHandler(NetMQMessage mqMsg)
        {
            var routingHash = new ConsistentHash(mqMsg[RoutingHashFrameIndex].Buffer);
            //Log($"Received msg for {(int)routingHash.Bytes[0]}");
            if (routingHash.IsBetween(Identity.RoutingHash, Successor.RoutingHash))
            {
                //Log($"Accepting");
                UnmarshallMsg(mqMsg);
            }
            else
            {
                //Log($"Forwarding");
                var hopCount = mqMsg[HopCountFrameIndex].ConvertToInt32();
                var routingTechnique = (RoutingTechnique) mqMsg[RoutingTechniqueIndex].ConvertToInt32();

                var newMqMsg = IncrementHopCount(mqMsg, hopCount);

                var routingHostAndPort = Successor.HostAndPort;
                if (routingTechnique == RoutingTechnique.Chord)
                {
                    var chordNodeIdentity = _routingTable.FindClosestPrecedingNode(routingHash);
                    routingHostAndPort = chordNodeIdentity.HostAndPort;
                }

                _socketCache[routingHostAndPort].SendMultipartMessage(newMqMsg);
            }
        }

        private static NetMQMessage IncrementHopCount(NetMQMessage mqMsg, int hopCount)
        {
            // NetMQ messages are immutable so we need to re-write to change the hop count
            // We'd never do this in production code. 
            var newMqMsg = new NetMQMessage();
            newMqMsg.Append(mqMsg[RoutingHashFrameIndex].Buffer);
            newMqMsg.Append(++hopCount);
            newMqMsg.Append(mqMsg[RoutingTechniqueIndex].Buffer);
            newMqMsg.Append(mqMsg[PayloadFrameIndex].Buffer);
            return newMqMsg;
        }

        void Log(string logMessage)
        {
            _logger?.Invoke($"{Identity.Identifier}\t{_clock.Now.ToString("hh:mm:ss.fff")}\t{logMessage}");
        }
        
        public NetMQMessage MarshallMsg(ConsistentHash destinationHash, Message msg)
        {
            var routingTechnique = (msg as IRoutingTechnique)?.Technique ?? RoutingTechnique.Successor;
            var json = _serializer.Serialize(msg);
            var hopCount = 0;

            var mqMsg = new NetMQMessage();
            mqMsg.Append(destinationHash.Bytes);
            mqMsg.Append(hopCount);
            mqMsg.Append((int)routingTechnique);
            mqMsg.Append(json);

            return mqMsg;
        }

        private void UnmarshallMsg(NetMQMessage mqMsg)
        {
            var msg = _serializer.Deserialize<Message>(json: mqMsg[PayloadFrameIndex].ConvertToString());
            var hopCount = mqMsg[HopCountFrameIndex].ConvertToInt32();
            var technique = (RoutingTechnique) mqMsg[RoutingTechniqueIndex].ConvertToInt32();
            Log($"{msg.TypeName()} Hops:{hopCount} Technique:{technique}");
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

        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            if (disposedValue) return;

            _actor.Dispose();
            _listeningSocket.Dispose();
            _socketCache.Dispose();

            disposedValue = true;
        }
        #endregion

        public void Handle(TerminateDht message)
        {
            // Send to all distinct entries in the routing table
 
            _actor.Stop();
        }
    }
}