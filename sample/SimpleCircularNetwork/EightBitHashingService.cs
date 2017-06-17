using System;
using CoreDht.Utils.Hashing;

namespace SimpleCircularNetwork
{
    /// <summary>
    /// This service will create a hash value with values ranging from 0-255. 
    /// Only useful for illustrative purposes...
    /// </summary>
    public class EightBitHashingService : IConsistentHashingService
    {
        public ConsistentHash GetConsistentHash(string key)
        {
            var basicHash = key.GetHashCode();
            var hash = Math.Abs(basicHash)%256;
            return new ConsistentHash(new [] {(byte)(hash) } );
        }
    }
}