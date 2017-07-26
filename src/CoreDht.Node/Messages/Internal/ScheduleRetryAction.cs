using CoreDht.Utils;
using CoreMemoryBus.Messages;

namespace CoreDht.Node.Messages.Internal
{
    public class ScheduleRetryAction : Message, ICorrelatedMessage<CorrelationId>
    {
        public ScheduleRetryAction(CorrelationId correlationId)
        {
            CorrelationId = correlationId;
        }

        public CorrelationId CorrelationId { get; }
    }
}