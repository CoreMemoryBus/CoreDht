using CoreDht.Node.Messages.Internal;
using CoreMemoryBus;

namespace CoreDht.Node
{
    partial class Node
    {
        public class TerminateHandler
            : IHandle<TerminateNode>
        {
            private readonly Node _node;

            public TerminateHandler(Node node)
            {
                _node = node;
            }

            public void Handle(TerminateNode message)
            {
                _node.Stop();
            }
        }
    }
}