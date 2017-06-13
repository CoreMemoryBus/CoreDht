using System;
using CoreDht.Utils.Hashing;
using NUnit.Framework;

namespace CoreDht.Utils.Test.Hashing
{
    [TestFixture]
    public class ConsistentHashFixture
    {
        [Test]
        public void TestConstruction()
        {
            var h0 = new ConsistentHash(new []{(byte)0});
            Assert.That(h0.BitCount, Is.EqualTo(8));
            Assert.That(h0.Rank, Is.EqualTo(1));
        }

        [Test]
        public void TestAdd()
        {
            var hMax = new ConsistentHash(new[] { byte.MaxValue });

            var hOne = ConsistentHash.One(1);
            var hZero = ConsistentHash.Zero(1);

            Assert.That(hZero + hZero, Is.EqualTo(hZero));
            Assert.That(hZero + hOne, Is.EqualTo(hOne));
            Assert.That(hOne + hOne, Is.EqualTo(new ConsistentHash(new[] { (byte)2 })));

            // Test wraparound case
            Assert.That(hMax + hOne, Is.EqualTo(hZero));
        }

        [Test]
        public void TestCompare()
        {
            var hOne = ConsistentHash.One(1);
            var hZero = ConsistentHash.Zero(1);
            var hX = ConsistentHash.One(1);
            var hTwo = new ConsistentHash(new [] {(byte)2});

            Assert.That(hOne == hX, Is.True);
            Assert.That(hZero < hOne, Is.True);
            Assert.That(hOne > hZero, Is.True);
            Assert.That(hOne <= hX, Is.True);
            Assert.That(hTwo >= hOne, Is.True);
        }

        [Test]
        public void TestVerifyRank()
        {
            var hOne = ConsistentHash.One(1);
            var hTwo = ConsistentHash.One(2);

            Assert.That(() => hOne + hTwo, Throws.InstanceOf<ConsistentHash.InconsistentRankException>());
            Assert.That(() => hOne < hTwo, Throws.InstanceOf<ConsistentHash.InconsistentRankException>());
            Assert.That(() => hOne > hTwo, Throws.InstanceOf<ConsistentHash.InconsistentRankException>());
            Assert.That(() => hOne == hTwo, Throws.InstanceOf<ConsistentHash.InconsistentRankException>());
            Assert.That(() => hOne <= hTwo, Throws.InstanceOf<ConsistentHash.InconsistentRankException>());
            Assert.That(() => hOne <= hTwo, Throws.InstanceOf<ConsistentHash.InconsistentRankException>());
        }

        [Test]
        public void TestShift()
        {
            var hOne = ConsistentHash.One(1);
            var hX = hOne << 10;
            Console.WriteLine($"{hX.ToHex()} {hOne.ToHex()}");
        }

        // Construction
        // One
        // Zero
        // New From Hex
        // New from Base58
        // Rank
        // Bitcount
        // Add
        // AddWithOverflow
        // Compare
        // Between*
        // Shift
        // ShiftWithOverflow

    }
}
