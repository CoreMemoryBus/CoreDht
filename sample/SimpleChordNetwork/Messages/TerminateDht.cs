using CoreDht.Utils.Hashing;
using CoreDht.Utils.Messages;

namespace SimpleChordNetwork.Messages
{
    public class TerminateDht : RoutableMessage
    {
        public TerminateDht(ConsistentHash routingTarget) : base(routingTarget)
        {}
    }
}