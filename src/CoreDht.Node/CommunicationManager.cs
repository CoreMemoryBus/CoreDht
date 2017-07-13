using CoreDht.Node.Messages;
using CoreDht.Utils.Hashing;
using CoreDht.Utils.Messages;
using CoreMemoryBus.Messages;
using CoreMemoryBus.Messaging;
using NetMQ;

namespace CoreDht.Node
{
    public class CommunicationManager : ICommunicationManager
    {
        private readonly IChordNode _node;
        private readonly INodeMarshaller _marshaller;
        private readonly ISocketCache _socketCache;
        private readonly IPublisher _publisher;

        public CommunicationManager(IChordNode node, INodeMarshaller marshaller, ISocketCache socketCache, IPublisher publisher)
        {
            _node = node;
            _marshaller = marshaller;
            _socketCache = socketCache;
            _publisher = publisher;
        }

        public void SendInternal(Message msg)
        {
            var replySocket = _socketCache[_node.Identity.HostAndPort];
            _marshaller.Send(msg, replySocket);
        }

        public void Send(PointToPointMessage msg)
        {
            var replySocket = _socketCache[msg.To.HostAndPort];
            _marshaller.Send(msg, replySocket);
        }

        public void Send(RoutableMessage msg)
        {
            var forwardingNode = _node.FindClosestPrecedingNode(msg.RoutingTarget);
            var forwardingSocket = _socketCache[forwardingNode.HostAndPort];
            var mqMsg = _marshaller.Marshall(msg);
            forwardingSocket.SendMultipartMessage(mqMsg);
        }

        public void Receive(NetMQMessage mqMsg)
        {
            var typeCode = mqMsg[0].ConvertToString();
            switch (typeCode)
            {
                case NodeMarshaller.RoutableMessage:
                    UnmarshalRoutableMsg(mqMsg);
                    break;
                case NodeMarshaller.PointToPointMessage:
                    UnMarshallPointToPointMsg(mqMsg);
                    break;
                case NodeMarshaller.InternalMessage:
                    UnMarshallMessage(mqMsg);
                    break;
                case NetMQActor.EndShimMessage:
                    _node.Stop();
                    break;
            }
        }

        private void UnmarshalRoutableMsg(NetMQMessage mqMsg)
        {
            var routingHash = new ConsistentHash(mqMsg[NodeMarshaller.RoutableFrameHashIndex].ToByteArray());
            if (routingHash.IsBetween(_node.Identity.RoutingHash, _node.Successor.RoutingHash))
            {
                RoutableMessage nodeMessage;
                _marshaller.Unmarshall(mqMsg, out nodeMessage);
                _publisher.Publish(nodeMessage);
            }
            else
            {
                var forwardingNode = _node.FindClosestPrecedingNode(routingHash);
                _socketCache[forwardingNode.HostAndPort].SendMultipartMessage(mqMsg);
            }
        }

        private void UnMarshallPointToPointMsg(NetMQMessage mqMsg)
        {
            PointToPointMessage nodeMessage;
            _marshaller.Unmarshall(mqMsg, out nodeMessage);
            _publisher.Publish(nodeMessage);
        }

        private void UnMarshallMessage(NetMQMessage mqMsg)
        {
            Message nodeMessage;
            _marshaller.Unmarshall(mqMsg, out nodeMessage);
            _publisher.Publish(nodeMessage);
        }
    }
}