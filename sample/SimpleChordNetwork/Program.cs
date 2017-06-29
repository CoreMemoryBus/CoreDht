using System;
using System.Collections.Generic;
using CoreDht.Node;
using CoreDht.Utils;
using CoreDht.Utils.Hashing;
using SimpleChordNetwork.Messages;

namespace SimpleChordNetwork
{
    /// <summary>
    /// In this example we use a trivial consistent hash (EightBitHashingService) to route messages through a simple circular network.
    /// When a message meets its target, it changes the state of that node. In this case we're "feeding animals" where specific animals
    /// are consistently created and fed in specific nodes. <br>
    /// The network is constructed externally and messages are routed in either a circular or chordwise fashion. We have a DHT, but it does not route
    /// efficiently, nor is it resilient or elastic. We can however demonstrate the increase in routing efficiency with the chord technique.
    /// We'll need to change the logic so that the network can calculate the next node instead of an external process.
    /// </summary>
    class Program
    {
        private readonly IConsistentHashingService _hashingService;
        private readonly SimpleNodeFactory _factory;

        const int MaxNodes = 10; // This is not a "thread efficient" example. We have 1 thread per node. 

        private void Run(string[] args)
        {
            using (var janitor = new DisposableStack())
            {
                var nodes = new List<SimpleChordNode>(MaxNodes);
                CreateAndSortNodes(janitor, nodes);
                AssignSuccessors(nodes);
                AssignRoutingTable(nodes);

                // Display nodes in hashed order
                nodes.ForEach(n =>
                {
                    Console.WriteLine($"{n.Identity} [{(int)n.Identity.RoutingHash.Bytes[0]},{n.Successor.RoutingHash.Bytes[0]})");
                });

                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();

                // Pick any node on the network as our entry point
                var sampleNode = nodes[5];
                Console.WriteLine($"Supplying messages at:{sampleNode.Identity}");

                // Feed the animals

                // Have a look at the routing path and number of hops.
                var routingId = _hashingService.GetConsistentHash(Keys.Yak);
                Console.WriteLine($"{Keys.Yak} Id:{(int)routingId.Bytes[0]}");
                // Observe the number of hops required to reach a target by technique
                sampleNode.SendToNetwork(new FeedAnimal(routingId, Keys.Yak) { Meals = 3 , Technique = RoutingTechnique.Successor});
                sampleNode.SendToNetwork(new FeedAnimal(routingId, Keys.Yak) { Meals = 1, Technique = RoutingTechnique.Chord});

                routingId = _hashingService.GetConsistentHash(Keys.Coyote);
                Console.WriteLine($"{Keys.Coyote} Id:{(int)routingId.Bytes[0]}");
                // Observe the number of hops required to reach a target by technique
                sampleNode.SendToNetwork(new FeedAnimal(routingId, Keys.Coyote) { Meals = 2, Technique = RoutingTechnique.Successor});
                sampleNode.SendToNetwork(new FeedAnimal(routingId, Keys.Coyote) { Meals = 6, Technique = RoutingTechnique.Chord});

                Console.ReadKey();
            }
        }

        private void AssignRoutingTable(List<SimpleChordNode> nodes)
        {
            // In a future implementation we need to be able to ask the ring network itself 
            // calculate these values for us instead of an external "network bootstrapper".
            for(int nodeIndex = 0; nodeIndex < nodes.Count; ++nodeIndex)
            {
                var startAt = nodes[nodeIndex].Identity.RoutingHash;
                var oneHash = startAt.One();
                var entries = new RoutingTableEntry[startAt.BitCount];
                for (int i = 0; i < entries.Length; ++i)
                {
                    var startEntryHash = startAt + (oneHash << i);
                    foreach (var node in nodes)
                    {
                        var nodeHash = node.Identity.RoutingHash;
                        if (nodeHash > startEntryHash)
                        {
                            entries[i] = new RoutingTableEntry(startEntryHash, node.Identity);
                            break;
                        }
                    }
                }

                nodes[nodeIndex].RoutingTable.Copy(entries);
            }
        }

        private static void AssignSuccessors(List<SimpleChordNode> nodes)
        {
            for (int i = MaxNodes - 1; i >= 0; --i)
            {
                nodes[i].Successor = i == MaxNodes - 1 ? nodes[0].Identity : nodes[i+1].Identity;
            }
        }

        private void CreateAndSortNodes(DisposableStack janitor, List<SimpleChordNode> nodes)
        {
            for (int i = 0; i < MaxNodes; ++i)
            {
                var newNode = janitor.Push(_factory.CreateNode($"SimpleNode:{i}"));
                nodes.Add(newNode);
            }

            nodes.Sort(CompareNodes);
        }

        private static int CompareNodes(SimpleChordNode x, SimpleChordNode y)
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
