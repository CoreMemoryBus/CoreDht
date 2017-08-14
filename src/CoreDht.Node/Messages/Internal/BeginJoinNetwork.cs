using CoreMemoryBus.Messages;

namespace CoreDht.Node.Messages.Internal
{
    public class BeginJoinNetwork : Message
    {
        public NodeInfo SeedNodeIdentity { get; }

        public BeginJoinNetwork(NodeInfo seedNodeIdentity)
        {
            SeedNodeIdentity = seedNodeIdentity;
        }
    }
}