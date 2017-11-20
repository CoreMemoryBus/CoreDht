using System.Security.Cryptography;

namespace CoreDht.Utils.Hashing
{
    public class HMACSHA256HashingService : HMACHashingService
    {
        public HMACSHA256HashingService() : base(HMACSHA256.Create())
        { }
    }
}