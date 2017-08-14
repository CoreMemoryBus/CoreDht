using System;
using CoreDht.Utils;
using CoreMemoryBus.Messaging;

namespace CoreDht.Node
{
    public class NodeHandlerContext
    {
        public Action<string> Logger { get; set; }
        public NodeInfo Identity { get; set; }
        public NodeConfiguration Configuration { get; set; }
        public ICommunicationManager CommunicationManager { get; set; }
        public IActionScheduler Scheduler { get; set; }
        public IExpiryTimeCalculator ExpiryTimeCalculator { get; set; }
        public ICorrelationIdFactory CorrelationIdFactory { get; set; }
        public MemoryBus MessageBus { get; set; }
    }
}