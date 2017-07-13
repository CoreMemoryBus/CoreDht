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
        /// JoinMinWait is the lower inclusive bound of the interval in millis
        /// </summary>
        public int JoinMinWait { get; set; }
        /// <summary>
        /// Joining the overlay network is a self-similar behaviour that could cause multiple network loops if nodes start up concurrently.
        /// By randomizing the start times within a range we can minimise the chance of this occurance.
        /// JoinMaxWait is the upper inclusive bound of the interval in millis
        /// </summary>
        public int JoinMaxWait { get; set; }
        /// <summary>
        /// To improve network resilience, a sequence of successors is stored, so it the immediate successor fails, then the next successor can immediately take it's place.
        /// This configuration sets the number of successors to be stored for this contingency.
        /// </summary>
        public int SuccessorCount { get; set; }
    }

    public class DefaultNodeConfiguration : NodeConfiguration
    {
        public DefaultNodeConfiguration()
        {
            JoinMinWait = 50;
            JoinMaxWait = 200;
            SuccessorCount = 2;
        }
    }
}
