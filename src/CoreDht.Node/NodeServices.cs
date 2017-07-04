using System;
using CoreDht.Utils.Hashing;

namespace CoreDht.Node
{
    public class NodeServices
    {
        /// <summary>
        /// Can be null
        /// </summary>
        public Action<string> Logger { get; set; }
        /// <summary>
        /// Never null when issued.
        /// </summary>
        public INodeSocketFactory SocketFactory { get; set; }
        /// <summary>
        /// Never null when issued
        /// </summary>
        public IConsistentHashingService ConsistentHashingService { get; set; }
    }

    public class DefaultInprocNodeServices : NodeServices
    {
        public DefaultInprocNodeServices()
        {
            SocketFactory = new InProcNodeSocketFactory();
            ConsistentHashingService = new Sha1HashingService();
        }
    }

    public class DefaultTcpNodeServices : NodeServices
    {
        public DefaultTcpNodeServices()
        {
            SocketFactory = new TcpNodeSocketFactory();
            ConsistentHashingService = new Sha1HashingService();
        }
    }
}