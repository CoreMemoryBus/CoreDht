using NetMQ;

namespace CoreDht.Node
{
    public interface INodeActor : IReceivingSocket, IOutgoingSocket
    { }
}