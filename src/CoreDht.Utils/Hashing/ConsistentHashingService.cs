using System.Security.Cryptography;
using System.Text;

namespace CoreDht.Utils.Hashing
{
    public abstract class ConsistentHashingService : IConsistentHashingService
    {
        private readonly HashAlgorithm _algo;

        protected ConsistentHashingService(HashAlgorithm algo)
        {
            _algo = algo;
        }

        public ConsistentHash GetConsistentHash(string key, string salt = "")
        {
            byte[] bytes = Encoding.ASCII.GetBytes(key + salt);
            bytes = _algo.ComputeHash(bytes);
            return new ConsistentHash(bytes);
        }
    }
}