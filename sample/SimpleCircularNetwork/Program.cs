using System;
using System.Collections.Generic;
using CoreDht.Utils;
using CoreDht.Utils.Hashing;
using SimpleCircularNetwork.Messages;

namespace SimpleCircularNetwork
{
    /// <summary>
    /// In this example we use a trivial consistent hash (EightBitHashingService) to route messages through a simple circular network.
    /// When a message meets its target, it changes the state of that node. In this case we're "feeding animals" where specific animals
    /// are consistently created and fed in specific nodes. <br>
    /// The network is constructed externally and messages are routed in a simple circular fashion. We have a DHT, but it does not route
    /// efficiently, nor is it resilient or elastic.
    /// </summary>
    class Program
    {
        private readonly IConsistentHashingService _hashingService;
        private readonly SimpleNodeFactory _factory;

        const int MaxNodes = 8; // This is not a "thread efficient" example. We have 1 thread per node. 

        private void Run(string[] args)
        {
            using (var janitor = new DisposableStack())
            {
                var nodes = new List<SimpleNode>(MaxNodes);
                CreateAndSortNodes(janitor, nodes);
                AssignSuccessors(nodes);

                // Tell each node to display it's address domain
                nodes.ForEach(n => n.SendToNode(new DisplayDomain()));

                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();

                // Pick any node on the network as our entry point
                var sampleNode = nodes[3];

                // Feed the animals
                // Observe that messages are consistently routed to the same object within each node on the network.
                var routingId = _hashingService.GetConsistentHash(Keys.Armadillo);
                Console.WriteLine($"{Keys.Armadillo} Id:{(int)routingId.Bytes[0]}");
                // Observe that the state of the animal object is changed. 
                sampleNode.SendToNetwork(new FeedAnimal(routingId, Keys.Armadillo) { Meals = 2 });
                sampleNode.SendToNetwork(new FeedAnimal(routingId, Keys.Armadillo) { Meals = 3 });

                routingId = _hashingService.GetConsistentHash(Keys.Yak);
                Console.WriteLine($"{Keys.Yak} Id:{(int)routingId.Bytes[0]}");
                // Observe that the state of the animal object is changed
                sampleNode.SendToNetwork(new FeedAnimal(routingId, Keys.Yak) { Meals = 3 });
                sampleNode.SendToNetwork(new FeedAnimal(routingId, Keys.Yak) { Meals = 1 });

                // Note how Coyote exists on the same node as Yak, but the states are never mixed up.
                routingId = _hashingService.GetConsistentHash(Keys.Coyote);
                Console.WriteLine($"{Keys.Coyote} Id:{(int)routingId.Bytes[0]}");
                sampleNode.SendToNetwork(new FeedAnimal(routingId, Keys.Coyote) { Meals = 2 });
                sampleNode.SendToNetwork(new FeedAnimal(routingId, Keys.Coyote) { Meals = 6 });

                ////Feed all the animals
                //foreach (var animal in Keys.Animals)
                //{
                //    routingId = _hashingService.GetConsistentHash(animal);
                //    sampleNode.SendToNetwork(new FeedAnimal(routingId, animal) { Meals = 1 });
                //}

                Console.ReadKey();

                sampleNode.SendToNode(new TerminateDht(sampleNode.Identity.RoutingHash));

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
