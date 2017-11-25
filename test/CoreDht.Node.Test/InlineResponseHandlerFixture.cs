using CoreDht.Node.Messages;
using CoreDht.Node.Messages.Internal;
using CoreDht.Utils;
using CoreMemoryBus.Messages;
using CoreMemoryBus.Messaging;
using NUnit.Framework;

namespace CoreDht.Node.Test
{
    [TestFixture]
    public class InlineResponseHandlerFixture
    {
        [Test]
        public void TestPerformAction()
        {
            int counter = 0;
            var correlationId = CorrelationId.NewId();

            var theBus = new MemoryBus();
            var responseHandler = new InlineResponseHandler(theBus, null);
            responseHandler.PerformAction(() => { ++counter; });
            Assert.That(counter, Is.EqualTo(0));

            responseHandler.Run(correlationId);
            Assert.That(counter, Is.EqualTo(1));
        }

        public class TestOperation : Message, ICorrelatedNodeMessage
        {
            public TestOperation(CorrelationId correlationId)
            {
                CorrelationId = correlationId;
            }

            public CorrelationId CorrelationId { get; }
        }

        public class TestResponse : Message, ICorrelatedNodeMessage
        {
            public TestResponse(CorrelationId correlationId)
            {
                CorrelationId = correlationId;
            }

            public CorrelationId CorrelationId { get; }
        }

        [Test]
        public void TestInlineHandlerOperationComplete()
        {
            bool performed = false;
            bool awaited = false;
            bool continued = false;
            bool finaled = false;
            var correlationId = CorrelationId.NewId();

            var theBus = new MemoryBus();
            var responseHandler = new InlineResponseHandler(theBus, null);
            responseHandler
                .PerformAction(() =>
                {
                    performed = true;
                    theBus.Publish(new TestOperation(correlationId));
                })
                .AndAwait(correlationId, (TestResponse a) =>
                {
                    awaited = true;
                })
                .ContinueWith(() =>
                {
                    continued = true;
                })
                .AndFinally(() =>
                {
                    finaled = true;
                });

            Assert.False(performed);
            Assert.False(awaited);
            Assert.False(continued);
            Assert.False(finaled);

            // When the bus encounters a TestOperation it will send a TestResponse immediately.
            var messageHandler = new InlineMessageHandler<TestOperation>(correlationId, () => theBus.Publish(new TestResponse(correlationId)));
            theBus.Subscribe(messageHandler);

            responseHandler.Run(correlationId);

            Assert.True(performed);
            Assert.True(awaited);
            Assert.True(continued);
            Assert.True(finaled);
        }

        [Test]
        public void TestInlineHandlerOperationCancelled()
        {
            bool performed = false;
            bool awaited = false;
            bool continued = false;
            bool finaled = false;
            var correlationId = CorrelationId.NewId();

            var theBus = new MemoryBus();
            var responseHandler = new InlineResponseHandler(theBus, null);
            responseHandler
                .PerformAction(() =>
                {
                    performed = true;
                    theBus.Publish(new TestOperation(correlationId));
                })
                .AndAwait(correlationId, (TestResponse a) =>
                {
                    awaited = true;
                })
                .ContinueWith(() =>
                {
                    continued = true;
                })
                .AndFinally(() =>
                {
                    finaled = true;
                });

            Assert.False(performed);
            Assert.False(awaited);
            Assert.False(continued);
            Assert.False(finaled);

            // When the bus encounters a TestOperation it will send a Cancel immediately.
            var messageHandler = new InlineMessageHandler<TestOperation>(correlationId, () => theBus.Publish(new CancelOperation(correlationId)));
            theBus.Subscribe(messageHandler);

            responseHandler.Run(correlationId);

            Assert.True(performed);

            // The operation never received a response, so it could not have been acknowledged.
            Assert.False(awaited);

            // The continuation is not invoked as the operation was cancelled
            Assert.False(continued);

            Assert.True(finaled);
        }
    }
}