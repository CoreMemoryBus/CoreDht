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
            var hX = hOne << 2;
            Assert.That(hX.ToHex(), Is.EqualTo("04"));
            // Overflow not accomodated
            Assert.That((hOne << 9).ToHex(), Is.EqualTo("00"));
        }

        [Test]
        public void TestBetweenExtension()
        {
            var hZero = ConsistentHash.Zero(1);
            var hTen = ConsistentHash.NewFromHex("0A");

            var hX = ConsistentHash.One(1);
            Assert.That(hX.IsBetween(hZero, hTen));

            // Lower Bound is exclusive
            hX = ConsistentHash.NewFromHex("00");
            Assert.That(!hX.IsBetween(hZero, hTen));

            // Upper Bound is inclusive
            hX = ConsistentHash.NewFromHex("0A");
            Assert.That(hX.IsBetween(hZero, hTen));

            // Wraparound case
            var hLower = ConsistentHash.NewFromHex("F0");
            var hUpper = ConsistentHash.NewFromHex("01");
            hX = ConsistentHash.NewFromHex("FF");
            Assert.That(hX.IsBetween(hLower, hUpper));

            hX = ConsistentHash.NewFromHex("F0");
            Assert.That(!hX.IsBetween(hLower, hUpper));
            hX = ConsistentHash.NewFromHex("01");
            Assert.That(hX.IsBetween(hLower, hUpper));
        }

        // New from Base58
    }
}
