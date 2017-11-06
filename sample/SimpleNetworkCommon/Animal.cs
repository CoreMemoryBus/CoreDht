using System;
using CoreDht.Utils.Hashing;
using CoreMemoryBus;
using CoreMemoryBus.Messaging;
using SimpleNetworkCommon.Messages;
using CoreMemoryBus.Handlers;

namespace SimpleNetworkCommon
{
    public class Animal 
        : RepositoryItem<ConsistentHash>
        , IAmTriggeredBy<FeedAnimal> // An instance will be instantiated by this message type
    {
        private readonly Action<string> _logger;
        public string Species { get; }

        public Animal(ConsistentHash correlationId, string species, Action<string> logger) : base(correlationId)
        {
            _logger = logger;
            Species = species;
        }

        public void Handle(FeedAnimal message)
        {
            Meals += message.Meals;
            _logger?.Invoke($"{Species} received {message.Meals} meals. Total:{Meals} meals Technique:{message.Technique}");
        }

        public int Meals { get; set; }
    }
}