using CoreDht.Node.Messages;
using CoreDht.Utils.Messages;
using CoreMemoryBus.Messages;
using NetMQ;

namespace CoreDht.Node
{
    /// <summary>
    /// An ICommunicationManager orchestrates the sending and receipt of messages across a network.
    /// It will manage the "negative-receipt" (aka NACK) messages and coordinate a retry in such circumstances 
    /// where a node will be in a transient state and is incapable of processing the original message.
    /// </summary>
    public interface ICommunicationManager
    {
        void SendInternal(Message msg);
        void Send(PointToPointMessage msg);
        void Send(RoutableMessage msg);

        void Receive(NetMQMessage mqMsg);
    }
}