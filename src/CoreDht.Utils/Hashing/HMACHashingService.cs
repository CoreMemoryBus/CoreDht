using System.Security.Cryptography;
using System.Text;

namespace CoreDht.Utils.Hashing
{
    public class HMACHashingService : IConsistentHashingService
    {
        HMAC _hashAlgo;

        protected HMACHashingService(HMAC hashAlgo)
        {
            _hashAlgo = hashAlgo;
        }

        public ConsistentHash GetConsistentHash(string key, string salt = "")
        {
            var saltBytes = Encoding.ASCII.GetBytes(salt);
            _hashAlgo.Key = saltBytes;

            var keyBytes = Encoding.ASCII.GetBytes(key);
            var bytes = _hashAlgo.ComputeHash(keyBytes);

            return new ConsistentHash(bytes);
        }
    }
}