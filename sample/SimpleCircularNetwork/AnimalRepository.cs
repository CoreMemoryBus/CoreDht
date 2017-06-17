using System;
using CoreDht.Utils;
using SimpleCircularNetwork.Messages;

namespace SimpleCircularNetwork
{
    public class AnimalRepository : RoutableRepository<Animal>
    {
        private readonly Action<string> _logger;

        public AnimalRepository(Action<string> logger)
        {
            _logger = logger;

            // Note the factory should respond to the same message types as "trigger types" if object state arrives from messages.
            RepoItemFactory = message => CreateAnimal((FeedAnimal) message);
        }

        Animal CreateAnimal(FeedAnimal msg)
        {
            _logger?.Invoke($"Creating:{msg.Animal}");
            return new Animal(msg.RoutingTarget, msg.Animal);
        }
    }
}