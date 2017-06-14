using System;
using CoreDht.Node;

namespace FirstExample
{
    public class MyAppNode : Node
    {
        public MyAppNode(string binding, Action<string> logger) : base(binding, logger)
        { }

        public new NodeActor Actor => base.Actor;
    }
}
