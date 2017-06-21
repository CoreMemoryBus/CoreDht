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

            for (int i = 0; i < tableLength; ++i)
            {
                var finger = routingHash + routingHash.One() << i;
                Entries[i] = new RoutingTableEntry(finger, identity);
            }
        }

        public static RoutingTableEntry[] CreateEntries(int entryCount, ConsistentHash nodeHash)
        {
            var entries = RoutingTable.CreateEntries(entryCount);
            for (int i = 0; i < entryCount; ++i)
            {
                var finger = nodeHash + nodeHash.One() << i;
                entries[i] = new RoutingTableEntry(finger, null);
            }

            return entries;
        }

        public NodeInfo FindClosestPrecedingFinger(ConsistentHash startingHash)
        {
            //Check finger tables
            for (int i = Entries.Length - 1; i >= 0; --i)
            {
                if (Entries[i].SuccessorIdentity.RoutingHash.IsBetween(Identity.RoutingHash, startingHash))
                {
                    return Entries[i].SuccessorIdentity;
                }
            }
            // Check successors

            return Identity;
        }
    }
}