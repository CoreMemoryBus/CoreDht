using CoreDht.Utils.Hashing;
using CoreDht.Utils.Messages;
using CoreMemoryBus.Messages;

namespace SimpleCircularNetwork.Messages
{
    public class FeedAnimal : RoutableMessage
    {
        public FeedAnimal(ConsistentHash routingId, string animal) : base(routingId)
        {
            Animal = animal;
        }

        public string Animal { get; }

        public int Meals { get; set; }
    }
}