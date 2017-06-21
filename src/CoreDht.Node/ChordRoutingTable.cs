using CoreDht.Utils.Hashing;

namespace CoreDht.Node
{
    public class ChordRoutingTable : RoutingTable
    {
        public NodeInfo Identity { get; }

        public ChordRoutingTable(NodeInfo identity, int tableLength) : base(tableLength)
        {
            Identity = identity;
            Init(identity, tableLength);
        }

        public void Init()
        {
            Init(Identity, Identity.RoutingHash.BitCount);
        }

        private void Init(NodeInfo identity, int tableLength)
        {
            var routingHash = identity.RoutingHash;
            var one = routingHash.One();

            for (int i = 0; i < tableLength; ++i)
            {
                var finger = routingHash + (one << i);
                Entries[i] = new RoutingTableEntry(finger, identity);
            }
        }

        public static RoutingTableEntry[] CreateEntries(int entryCount, ConsistentHash nodeHash)
        {
            var entries = RoutingTable.CreateEntries(entryCount);
            var one = nodeHash.One();

            for (int i = 0; i < entryCount; ++i)
            {
                var finger = nodeHash + (one << i);
                entries[i] = new RoutingTableEntry(finger, null);
            }

            return entries;
        }

        public NodeInfo FindClosestPrecedingFinger(ConsistentHash startingHash)
        {
            for (int i = Entries.Length - 1; i >= 0; --i)
            {
                var chordNode = Entries[i].SuccessorIdentity;
                var chordHash = chordNode.RoutingHash;
                if (chordHash.IsBetween(Identity.RoutingHash, startingHash))
                {
                    return chordNode;
                }
            }

            return Identity;
        }
    }
}