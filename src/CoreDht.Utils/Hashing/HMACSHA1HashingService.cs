using System.Security.Cryptography;

namespace CoreDht.Utils.Hashing
{
    public class HMACSHA1HashingService : HMACHashingService
    {
        public HMACSHA1HashingService() : base(HMACSHA1.Create())
        { }
    }
}