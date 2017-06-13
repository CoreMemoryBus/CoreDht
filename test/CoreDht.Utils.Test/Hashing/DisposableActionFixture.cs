using NUnit.Framework;

namespace CoreDht.Utils.Test.Hashing
{
    [TestFixture]

    public class DisposableActionFixture
    {
        [Test]
        public void TestExitAction()
        {
            bool exited = false;
            using (new DisposableAction(() => exited = true))
            {
               Assert.That(exited, Is.False); 
            }
            Assert.That(exited, Is.True);
        }

        [Test]
        public void TestEntryAndExit()
        {
            bool entered = false;
            bool exited = false;
            using (new DisposableAction(() => entered = true, () => exited = true))
            {
                Assert.That(entered, Is.True);
                Assert.That(exited, Is.False);
            }
            Assert.That(exited,Is.True);
        }
    }
}