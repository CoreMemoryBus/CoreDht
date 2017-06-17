using CoreDht.Utils.Hashing;

namespace SimpleCircularNetwork.Messages
{
    public interface IRoutableMessage
    {
        ConsistentHash RoutingId { get; }
    }
}