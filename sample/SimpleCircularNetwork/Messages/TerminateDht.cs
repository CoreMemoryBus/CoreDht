using CoreDht.Utils.Hashing;
using CoreDht.Utils.Messages;

namespace SimpleCircularNetwork.Messages
{
    public class TerminateDht : RoutableMessage
    {
        public TerminateDht(ConsistentHash routingTarget) : base(routingTarget)
        {}
    }
}