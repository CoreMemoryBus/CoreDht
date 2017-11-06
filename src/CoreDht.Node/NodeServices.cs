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
        /// <summary>
        /// We introduce a RandomNUmberGenerator typically for "self similar" operations to ensure that such detrimental effects can be avoided 
        /// by introducing an element of random behaviour.
        /// </summary>
        public IRandomNumberGenerator RandomNumberGenerator { get; set; }

        public IMessageBusFactory MessageBusFactory { get; set; }
    }

    public class DefaultNodeServices : NodeServices
    {
        public DefaultNodeServices()
        {
            Clock = new UtcClock();
            TimerFactory = new ActionTimerFactory();
            ConsistentHashingService = new Sha1HashingService();
            CorrelationIdFactory = new CorrelationIdFactory();
            RandomNumberGenerator = new RandomNumberGenerator(CorrelationIdFactory);
            ExpiryTimeCalculator = new ExpiryTimeCalculator(Clock, RandomNumberGenerator);
            MessageBusFactory = new MessageBusFactory();
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