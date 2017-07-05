using CoreDht.Utils.Hashing;

namespace CoreDht.Utils.Messages
{
    public interface IRoutableMessage
    {
        ConsistentHash RoutingTarget { get; }
    }
}