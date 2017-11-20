using System.Security.Cryptography;

namespace CoreDht.Utils.Hashing
{
    public class HMACMD5HashingService : HMACHashingService
    {
        public HMACMD5HashingService() : base(HMACMD5.Create())
        { }
    }
}