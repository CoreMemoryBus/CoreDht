using CoreDht.Utils.Hashing;

namespace CoreDht.Node
{
    public interface IChordNode
    {
        NodeInfo Identity { get; }
        NodeInfo Successor { get; }
        NodeInfo FindClosestPrecedingNode(ConsistentHash startingHash);
        void Stop();
    }
}