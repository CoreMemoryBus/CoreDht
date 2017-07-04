using NUnit.Framework;

namespace CoreDht.Node.Test
{
    [TestFixture]
    public class NodeFixture
    {
        public class TestNode : Node
        {
            public TestNode()
                : base("inprocAddr", "TestNode", new DefaultInprocNodeServices())
            {}
        }

        [Test]
        public void WhenANodeIsInstantiated()
        {
            var node = new TestNode();
            Assert.That(node.Predecessor, Is.Not.Null);
            Assert.That(node.Predecessor, Is.EqualTo(node.Identity));

            Assert.That(node.Successor, Is.Not.Null);
            Assert.That(node.Successor, Is.EqualTo(node.Identity));
        }
    }
}
