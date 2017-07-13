using CoreDht.Node.Messages;
using CoreDht.Utils.Messages;
using CoreMemoryBus.Messages;
using NetMQ;

namespace CoreDht.Node
{
    public interface ICommunicationManager
    {
        void SendInternal(Message msg);

        void Receive(NetMQMessage mqMsg);
        void Send(PointToPointMessage msg);
        void Send(RoutableMessage msg);
    }
}