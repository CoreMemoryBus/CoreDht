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
        public IActionTimerFactory TimerFactory { get; set; }
        /// <summary>
        /// The ExpiryTimeCalculator is used to calculate the expiry time of events and ephemeral messages.
        /// </summary>
        public IExpiryTimeCalculator ExpiryTimeCalculator { get; set; }
        /// <summary>
        /// Sometimes collections of messages pertain to a common subject. A CorrelationId helps identify the common subject between them. 
        /// The CorrelationIdFactory generates unique Id's that are used to identify the common subject.
        /// </summary>
        public ICorrelationIdFactory CorrelationIdFactory { get; set; }
    }

    public class DefaultNodeServices : NodeServices
    {
        public DefaultNodeServices()
        {
            Clock = new UtcClock();
            TimerFactory = new ActionTimerFactory();
            ConsistentHashingService = new Sha1HashingService();
            ExpiryTimeCalculator = new ExpiryTimeCalculator(Clock);
            CorrelationIdFactory = new CorrelationIdFactory();
        }
    }

    public class DefaultInprocNodeServices : DefaultNodeServices
    {
        public DefaultInprocNodeServices()
        {
            SocketFactory = new InProcNodeSocketFactory();
        }
    }

    public class DefaultTcpNodeServices : DefaultNodeServices
    {
        public DefaultTcpNodeServices()
        {
            SocketFactory = new TcpNodeSocketFactory();
        }
    }
}