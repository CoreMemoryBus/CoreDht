using CoreDht.Node.Messages;
using CoreDht.Utils.Messages;
using CoreMemoryBus.Messages;
using NetMQ;

namespace CoreDht.Node
{
    /// <summary>
    /// A NodeMarshaller is responsible for marshalling messages from a socket-and-protocol format to 
    /// a data structure suited to broadcast on the message bus (and vice-versa).
    /// </summary>
    public interface INodeMarshaller
        : INodeMarshaller<Message>
        , INodeMarshaller<PointToPointMessage>
        , INodeMarshaller<RoutableMessage>
    { }

    public interface INodeMarshaller<T> where T : Message
    {
        NetMQMessage Marshall(T msg);
        void Unmarshall(NetMQMessage mqMessage, out T result);
        void Send(T msg, IOutgoingSocket forwardingSocket);
    }
}