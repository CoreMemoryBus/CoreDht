using System;
using CoreDht.Utils;
using CoreDht.Utils.Hashing;

namespace CoreDht.Node
{
    public class NodeServices
    {
        public IUtcClock Clock { get; set; }
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

        public ICommunicationManagerFactory CommunicationManagerFactory { get; set; }
    }

    public class DefaultInprocNodeServices : NodeServices
    {
        public DefaultInprocNodeServices()
        {
            Clock = new UtcClock();
            SocketFactory = new InProcNodeSocketFactory();
            ConsistentHashingService = new Sha1HashingService();
        }
    }

    public class DefaultTcpNodeServices : NodeServices
    {
        public DefaultTcpNodeServices()
        {
            Clock = new UtcClock();
            SocketFactory = new TcpNodeSocketFactory();
            ConsistentHashingService = new Sha1HashingService();
        }
    }
}