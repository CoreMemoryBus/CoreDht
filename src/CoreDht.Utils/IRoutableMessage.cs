using CoreDht.Utils.Hashing;

namespace CoreDht.Utils
{
    public interface IRoutableMessage
    {
        ConsistentHash RoutingTarget { get; }
    }
}