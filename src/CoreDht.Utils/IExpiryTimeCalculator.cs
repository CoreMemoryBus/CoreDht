using System;

namespace CoreDht.Utils
{
    public interface IExpiryTimeCalculator
    {
        DateTime CalcExpiry(int timeoutMilliSec);
        DateTime CalcRandomExpiry(int timeoutMillisec, int variationMillisec);
    }
}