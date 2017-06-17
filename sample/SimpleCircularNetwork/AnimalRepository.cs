using System;
using CoreDht.Utils;
using CoreDht.Utils.Hashing;
using SimpleCircularNetwork.Messages;

namespace SimpleCircularNetwork
{
    public class AnimalRepository : RoutableRepository<Animal>
    {
        public AnimalRepository() : base(x => CreateAnimal((FeedAnimal)x))
        {}

        static Animal CreateAnimal(FeedAnimal msg)
        {
            Console.WriteLine($"Creating:{msg.Animal}");
            return new Animal(msg.RoutingTarget, msg.Animal);
        }
    }
}