using System;
using System.Threading;
using CoreDht.Node;
using CoreDht.Utils.Hashing;

namespace FirstExample
{
    public class MyAppNode : Node
    {
        public class MyServices : DefaultInprocNodeServices
        {
            public MyServices()
            {
                Logger = Console.WriteLine;
            }
        }

        public MyAppNode(string hostAndPort, string identifier)
            : base(hostAndPort, identifier, new DefaultNodeConfiguration(), new MyServices())
        {
            Actor.Start();
        }

        public new NodeActor Actor => base.Actor;
    }
}
