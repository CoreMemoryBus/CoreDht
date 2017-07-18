using CoreDht.Node.Messages.Internal;
using CoreMemoryBus;

namespace CoreDht.Node
{
    partial class Node
    {
        public class LifetimeHandler
            : IHandle<NodeInitialised>
            , IHandle<TerminateNode>
        {
            private readonly Node _node;

            public LifetimeHandler(Node node)
            {
                _node = node;
            }

            public void Handle(NodeInitialised message)
            {
                _node.OnInitialised();
            }

            public void Handle(TerminateNode message)
            {
                _node.Stop();
            }
        }
    }
}