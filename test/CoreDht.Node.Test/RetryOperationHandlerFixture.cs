using CoreDht.Node.Messages.Internal;
using CoreDht.Utils;
using CoreMemoryBus.Messaging;
using NUnit.Framework;

namespace CoreDht.Node.Test
{
    [TestFixture]
    public class RetryOperationHandlerFixture 
    {
        [Test]
        public void TestInvoke()
        {
            int counter = 0;
            var theBus = new MemoryBus();
            var correlationId = CorrelationId.NewId();

            // We'll increment a counter to check it incerements the correct number of times. (Count+1)
            var op = new RetryOperationHandler(theBus, correlationId, () => { ++counter; }, 3);
            var handler = new InlineMessageHandler<ScheduleRetryAction>(correlationId, () => op.Invoke());
            theBus.Subscribe(handler);

            op.Invoke();

            Assert.That(counter, Is.EqualTo(4));
        }
    }
}