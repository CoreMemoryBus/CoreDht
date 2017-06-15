using System;
using System.Collections.Generic;
using System.Linq;
using CoreDht.Node;
using CoreDht.Utils;
using CoreDht.Utils.Hashing;

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
            }
        }

        private static void AssignSuccessors(List<SimpleNode> nodes)
        {
            for (int i = MaxNodes - 1; i >= 0; --i)
            {
                if (i == MaxNodes - 1)
                {
                    nodes[i].Successor = nodes[0].Identity;
                }
                else
                {
                    nodes[i].Successor = nodes[i+1].Identity;
                }
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
            _hashingService = new SimpleHashingService();
            _factory = new SimpleNodeFactory(_hashingService);
        }

        static void Main(string[] args)
        {
            var theApp = new Program();
            theApp.Run(args);
        }
    }

    /// <summary>
    /// This service will create a hash value with values ranging from 0-255. 
    /// Only useful for illustrative purposes...
    /// </summary>
    public class SimpleHashingService : IConsistentHashingService
    {
        public ConsistentHash GetConsistentHash(string key)
        {
            var basicHash = key.GetHashCode();
            var hash = Math.Abs(basicHash)%256;
            return new ConsistentHash(new [] {(byte)(hash) } );
        }
    }

    public static class Data
    {
        public static string[] Animals =
        {
            "Abyssinian",
            "Alligator",
            "Anteater",
            "Armadillo",
            "Axolotl",
            "Barb",
            "Barracuda",
            "Bison",
            "Buffalo",
            "Bulldog",
            "Butterfly",
            "Cassowary",
            "Cattle",
            "Centipede",
            "Chinook",
            "Clam",
            "Coati",
            "Cockroach",
            "Coral",
            "Coyote",
            "Crow",
            "Cuttlefish",
            "Dhole",
            "Dodo",
            "Dragonfly",
            "Eagle",
            "Earwig",
            "Eland",
            "Ferret",
            "Gibbon",
            "Giraffe",
            "Goldfinch",
            "Grasshopper",
            "Greyhound",
            "Gull",
            "Hare",
            "Hedgehog",
            "Herring",
            "Himalayan",
            "Ibis",
            "Jaguar",
            "Kiwi",
            "Lemming",
            "Lemur",
            "Lynx",
            "Mallard",
            "Mastiff",
            "Meerkat",
            "Mole",
            "Molly",
            "Mosquito",
            "Mule",
            "Nightingale",
            "Okapi",
            "Olm",
            "Peacock",
            "Pony",
            "Quetzal",
            "Quokka",
            "Rail",
            "Rat",
            "Rottweiler",
            "Ruff",
            "Sandpiper",
            "Seahorse",
            "Sheep",
            "Snake",
            "Sparrow",
            "Sponge",
            "Squirrel",
            "Tiffany",
            "Tiger",
            "Toad",
            "Tuatara",
            "Turkey",
            "Turtle",
            "Viper",
            "Walrus",
            "Weasel",
            "Whippet",
            "Wombat",
            "Woodpecker",
            "Wren",
            "Yak",
        };
    }
}
