using System.Threading;

namespace CoreDht.Node
{
    /// <summary>
    /// This is a collection of settings common to all nodes within the network
    /// </summary>
    public class NodeConfiguration
    {
        /// <summary>
        /// RoutingSalt is a value that is concatenated onto an identifier before hashing to consistently alter the default hashing output.
        /// Theoretically, an address on the overlay network cannot be calculated unless you also have the salt value.
        /// </summary>
        public string RoutingSalt { get; set; }
        /// <summary>
        /// Joining the overlay network is a self-similar behaviour that could cause multiple network loops if nodes start up concurrently.
        /// By randomizing the start times within a range we can minimise the chance of this occurance.
        /// JoinWaitMin is the lower inclusive bound of the interval in millis
        /// </summary>
        public int JoinWaitMin { get; set; }
        /// <summary>
        /// Joining the overlay network is a self-similar behaviour that could cause multiple network loops if nodes start up concurrently.
        /// By randomizing the start times within a range we can minimise the chance of this occurance.
        /// JoinWaitVariation is the random variation above the minimum wait of the interval in millis
        /// </summary>
        public int JoinWaitVariation { get; set; }
        /// <summary>
        /// To improve network resilience, a sequence of successors is stored, so it the immediate successor fails, then the next successor can immediately take it's place.
        /// This configuration sets the number of successors to be stored for this contingency.
        /// </summary>
        public int SuccessorCount { get; set; }
        /// <summary>
        /// AwaitTimeout is the time in milliseconds to wait for an operation to complete before timing out. In practice, the reply handler will be removed from the message bus 
        /// thus ignoring the reply.
        /// </summary>
        public int AwaitTimeout { get; set; }
        /// <summary>
        /// AckTimeout is the time in milliseconds a time sensitive operation is extended by before timing out.
        /// </summary>
        public int AckTimeout { get; set; }
        /// <summary>
        /// To be deprecated. We'll switch this over to a multicast beacon.
        /// To connect 
        /// </summary>
        public NodeInfo SeedNodeIdentity { get; set; }

        public int RetryTimeout { get; set; }
        public int RetryCount { get; set; }
    }

    public class DefaultNodeConfiguration : NodeConfiguration
    {
        public DefaultNodeConfiguration()
        {
            JoinWaitMin = 50;
            JoinWaitVariation = 200;
            SuccessorCount = 1;
            AwaitTimeout = 200;
            AckTimeout = 50;
            RetryTimeout = Timeout.Infinite;
            RetryCount = CoreDht.Node.RetryCount.Infinite;
        }
    }
}
