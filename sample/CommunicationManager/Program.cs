using System;
using System.Collections.Generic;
using CoreDht.Node;
using CoreDht.Utils;
using CoreDht.Utils.Hashing;
using SimpleNetworkCommon;
using SimpleNetworkCommon.Messages;

namespace CommunicationManager
{
    class Program
    {
        private readonly IConsistentHashingService _hashingService;
        private readonly AnimalNodeFactory _factory;
        private NodeServicesFactory _nodeServicesFactory;

        const int MaxNodes = 10;

        private void Run(string[] args)
        {
            using (var janitor = new DisposableStack())
            {
                var nodes = new List<AnimalNode>(MaxNodes);
                CreateAndSortNodes(janitor, nodes);
                AssignSuccessors(nodes);
                AssignRoutingTable(nodes);

                nodes.ForEach(n =>
                {
                    Console.WriteLine($"{n.Identity} [{(int)n.Identity.RoutingHash.Bytes[0]},{n.Successor.RoutingHash.Bytes[0]})");
                });

                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();

                var sampleNode = nodes[5];
                Console.WriteLine($"Routing table for {sampleNode.Identity}");
                foreach (var entry in sampleNode.RoutingTable.Entries)
                {
                    Console.WriteLine($"StartAt:{(int)entry.StartValue.Bytes[0]} Successor:{entry.SuccessorIdentity}");
                }

                nodes.ForEach(n => n.Start());

                Console.ReadKey();

                Console.WriteLine($"Supplying messages at:{sampleNode.Identity}");

                // Feed the animals
                var routingId = _hashingService.GetConsistentHash(Keys.Yak);
                Console.WriteLine($"{Keys.Yak} Id:{(int)routingId.Bytes[0]}");
                // Observe the number of hops required to reach a target by technique
                sampleNode.SendToNetwork(new FeedAnimal(routingId, Keys.Yak) { Meals = 3 });
                sampleNode.SendToNetwork(new FeedAnimal(routingId, Keys.Yak) { Meals = 1 });

                routingId = _hashingService.GetConsistentHash(Keys.Coyote);
                Console.WriteLine($"{Keys.Coyote} Id:{(int)routingId.Bytes[0]}");
                sampleNode.SendToNetwork(new FeedAnimal(routingId, Keys.Coyote) { Meals = 2 });
                sampleNode.SendToNetwork(new FeedAnimal(routingId, Keys.Coyote) { Meals = 6 });

                Console.ReadKey();
            }
        }

        private void AssignRoutingTable(List<AnimalNode> nodes)
        {
            for (int nodeIndex = 0; nodeIndex < nodes.Count; ++nodeIndex)
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
                    // if nothing was found (wraparound)
                    if (entries[i] == null)
                    {
                        entries[i] = new RoutingTableEntry(startEntryHash, nodes[0].Identity);
                    }
                }

                nodes[nodeIndex].RoutingTable.Copy(entries);
            }
        }

        private void AssignSuccessors(List<AnimalNode> nodes)
        {
            for (int i = MaxNodes - 1; i >= 0; --i)
            {
                nodes[i].Successor = i == MaxNodes - 1 ? nodes[0].Identity : nodes[i + 1].Identity;
            }
        }

        private void CreateAndSortNodes(DisposableStack janitor, List<AnimalNode> nodes)
        {
            for (int i = 0; i < MaxNodes; ++i)
            {
                var newNode = janitor.Push(_factory.CreateNode($"AnimalNode:{i}"));
                nodes.Add(newNode);
            }

            nodes.Sort(CompareNodes);
        }

        private static int CompareNodes(AnimalNode x, AnimalNode y)
        {
            if (x.Identity.RoutingHash < y.Identity.RoutingHash) return -1;
            return x.Identity.RoutingHash == y.Identity.RoutingHash ? 0 : 1;
        }

        Program()
        {
            _hashingService = new EightBitHashingService();
            _nodeServicesFactory = new NodeServicesFactory(_hashingService);
            _factory = new AnimalNodeFactory(_nodeServicesFactory);
        }

        static void Main(string[] args)
        {
            var theApp = new Program();
            theApp.Run(args);
        }
    }
}
