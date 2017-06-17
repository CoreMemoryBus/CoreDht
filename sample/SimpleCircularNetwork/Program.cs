using System;
using System.Collections.Generic;
using CoreDht.Utils;
using CoreDht.Utils.Hashing;
using SimpleCircularNetwork.Messages;

namespace SimpleCircularNetwork
{
    class Program
    {
        private readonly IConsistentHashingService _hashingService;
        private readonly SimpleNodeFactory _factory;

        const int MaxNodes = 8;

        private void Run(string[] args)
        {
            using (var janitor = new DisposableStack())
            {
                var nodes = new List<SimpleNode>(MaxNodes);
                CreateAndSortNodes(janitor, nodes);
                AssignSuccessors(nodes);

                // Tell each node to display it's address domain
                nodes.ForEach(n => n.SendToNode(new DisplayDomain()));

                var sampleNode = nodes[0];
                var routingId = _hashingService.GetConsistentHash("Armadillo");
                sampleNode.SendToNetwork(new FeedAnimal(routingId, "Armadillo") { Meals = 2 });
                sampleNode.SendToNetwork(new FeedAnimal(routingId, "Armadillo") { Meals = 3 });

                Console.ReadKey();
            }
        }

        private static void AssignSuccessors(List<SimpleNode> nodes)
        {
            for (int i = MaxNodes - 1; i >= 0; --i)
            {
                nodes[i].Successor = i == MaxNodes - 1 ? nodes[0].Identity : nodes[i+1].Identity;
            }
        }

        private void CreateAndSortNodes(DisposableStack janitor, List<SimpleNode> nodes)
        {
            for (int i = 0; i < MaxNodes; ++i)
            {
                var newNode = janitor.Push(_factory.CreateNode($"SimpleNode:{i}"));
                nodes.Add(newNode);
            }

            nodes.Sort(CompareNodes);
        }

        private static int CompareNodes(SimpleNode x, SimpleNode y)
        {
            if (x.Identity.RoutingHash < y.Identity.RoutingHash) return -1;
            return x.Identity.RoutingHash == y.Identity.RoutingHash ? 0 : 1;
        }

        Program()
        {
            _hashingService = new EightBitHashingService();
            _factory = new SimpleNodeFactory(_hashingService);
        }

        static void Main(string[] args)
        {
            var theApp = new Program();
            theApp.Run(args);
        }
    }
}
