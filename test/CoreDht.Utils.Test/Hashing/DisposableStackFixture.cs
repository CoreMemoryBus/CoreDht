using NUnit.Framework;

namespace CoreDht.Utils.Test.Hashing
{
    [TestFixture]

    public class DisposableStackFixture
    {
        [Test]
        public void TestStack()
        {
            int i = 0;
            using (var newStack = new DisposableStack())
            {
                var d0 = newStack.Push(new DisposableAction(() => ++i));
                var d1 = newStack.Push(new DisposableAction(() => ++i));
                var d2 = newStack.Push(new DisposableAction(() => ++i));
                var d3 = newStack.Push(new DisposableAction(() => ++i));

                Assert.That(i, Is.EqualTo(0));
            }
            Assert.That(i, Is.EqualTo(4));
        }
    }
}