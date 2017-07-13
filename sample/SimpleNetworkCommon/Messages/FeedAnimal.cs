using CoreDht.Utils.Hashing;
using CoreDht.Utils.Messages;

namespace SimpleNetworkCommon.Messages
{
    public class FeedAnimal : RoutableMessage, IRoutingTechnique
    {
        public FeedAnimal(ConsistentHash routingId, string animal) : base(routingId)
        {
            Animal = animal;
            Technique = RoutingTechnique.Chord;
        }

        public string Animal { get; }

        public int Meals { get; set; }

        public RoutingTechnique Technique { get; set; }
    }
}