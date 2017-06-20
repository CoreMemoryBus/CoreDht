using CoreDht.Utils.Hashing;
using CoreDht.Utils.Messages;
using CoreMemoryBus.Messages;

namespace SimpleChordNetwork.Messages
{
    public class FeedAnimal : RoutableMessage, IRoutingTechnique
    {
        public FeedAnimal(ConsistentHash routingId, string animal) : base(routingId)
        {
            Animal = animal;
        }

        public string Animal { get; }

        public int Meals { get; set; }

        public RoutingTechnique Technique { get; set; }
    }
}