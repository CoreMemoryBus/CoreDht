using System;
using CoreDht.Utils.Hashing;
using CoreMemoryBus;
using CoreMemoryBus.Messaging;
using SimpleCircularNetwork.Messages;

namespace SimpleCircularNetwork
{
    public class Animal 
        : RepositoryItem<ConsistentHash>
        , IAmTriggeredBy<FeedAnimal> // An instance will be instantiated by this message type
    {
        public string Species { get; }

        public Animal(ConsistentHash correlationId, string species) : base(correlationId)
        {
            Species = species;
        }

        public void Handle(FeedAnimal message)
        {
            Meals += message.Meals;
            Console.WriteLine($"{Species} received {message.Meals} meals. Total:{Meals} meals");
        }

        public int Meals { get; set; }
    }
}