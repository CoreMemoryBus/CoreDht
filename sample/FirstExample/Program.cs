using System;
using CoreDht.Node;
using CoreDht.Utils.Hashing;
using NetMQ;

namespace FirstExample
{
    class Program
    {
        void Run(string[] args)
        {
            var firstNode = new MyAppNode("localhost:9000", "MyApp");
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
            firstNode.Actor.SendFrame(NetMQActor.EndShimMessage);
            Console.ReadKey();
        }

        static void Main(string[] args)
        {
            var theApp = new Program();
            theApp.Run(args);
        }

        Program()
        {}
    }
}
