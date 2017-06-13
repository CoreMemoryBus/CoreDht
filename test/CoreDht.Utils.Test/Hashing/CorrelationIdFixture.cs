using System;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace CoreDht.Utils.Test.Hashing
{
    [TestFixture]
    public class CorrelationIdFixture
    {
        const string Base58CheckedEmpty = "11111111111111114Ki9Gx";
        const string EmptyGuid = "00000000-0000-0000-0000-000000000000";

        [Test]
        public void TestConstruction()
        {
            var c0 = new CorrelationId(Base58CheckedEmpty);
            Assert.That(c0, Is.EqualTo(CorrelationId.Empty));
        }

        [Test]
        public void TestEquals()
        {
            var c0 = new CorrelationId(Base58CheckedEmpty);
            Assert.That(c0.Equals(CorrelationId.Empty), Is.True);
            Assert.That(c0.Equals(Guid.Empty), Is.True);
        }
    }
}