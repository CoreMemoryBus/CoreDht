using System;
using System.Threading;
using CoreDht.Node;
using CoreDht.Utils.Hashing;

namespace FirstExample
{
    public class MyAppNode : Node
    {
        public class DefaultServices : DefaultInprocNodeServices
        {
            public DefaultServices()
            {
                Logger = Console.WriteLine;
            }
        }

        public MyAppNode(string hostAndPort, string identifier)
            : base(hostAndPort, identifier, new DefaultNodeConfiguration(), new DefaultServices())
        {
            Actor.Start();
        }

        public new NodeActor Actor => base.Actor;
    }
}
