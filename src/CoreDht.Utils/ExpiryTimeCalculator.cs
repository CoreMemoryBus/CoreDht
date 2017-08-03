using System;

namespace CoreDht.Utils
{
    public class ExpiryTimeCalculator : IExpiryTimeCalculator
    {
        private readonly IUtcClock _clock;
        private readonly IRandomNumberGenerator _rng;

        public ExpiryTimeCalculator(IUtcClock clock, IRandomNumberGenerator rng)
        {
            _clock = clock;
            _rng = rng;
        }

        public DateTime CalcExpiry(int timeoutMilliSec)
        {
            return _clock.Now + new TimeSpan(0, 0, 0, 0, timeoutMilliSec);
        }

        public DateTime CalcRandomExpiry(int timeoutMillisec, int variationMillisec)
        {
            timeoutMillisec = _rng.Next(timeoutMillisec, timeoutMillisec + variationMillisec);
            return CalcExpiry(timeoutMillisec);
        }
    }
}