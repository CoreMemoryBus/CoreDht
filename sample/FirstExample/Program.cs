using System;
using CoreDht.Node;
using NetMQ;

namespace FirstExample
{
    class Program
    {
        void Run(string[] args)
        {
            var firstNode = new MyAppNode("@inproc://localhost:9000", Console.WriteLine);
            Console.ReadKey();
            firstNode.Actor.SendFrame(NetMQActor.EndShimMessage);
            Console.ReadKey();
        }

        static void Main(string[] args)
        {
            var theApp = new Program();
            theApp.Run(args);
        }
    }

    public class MyAppNode : Node
    {
        public MyAppNode(string binding, Action<string> logger) : base(binding, logger)
        { }
    }
}
