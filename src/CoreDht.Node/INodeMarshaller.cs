using CoreMemoryBus.Messages;
using NetMQ;

namespace CoreDht.Node
{
    public interface INodeMarshaller<T> where T : Message
    {
        NetMQMessage Marshall(T msg);
        void Unmarshall(NetMQMessage mqMessage, out T result);
        void Send(T msg, IOutgoingSocket forwardingSocket);
    }
}