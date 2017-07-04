namespace CoreDht.Node
{
    /// <summary>
    /// This is a collection of settings common to all nodes within the network
    /// </summary>
    public class NodeConfiguration
    {
        public int JoinMinWait { get; set; }
        public int JoinMaxWait { get; set; }
    }

    public class DefaultNodeConfiguration : NodeConfiguration
    {
        public DefaultNodeConfiguration()
        {
            JoinMinWait = 50;
            JoinMaxWait = 200;
        }
    }
}
