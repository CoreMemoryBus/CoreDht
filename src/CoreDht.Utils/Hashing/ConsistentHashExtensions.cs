namespace CoreDht.Utils.Hashing
{
    public static class ConsistentHashExtensions
    {
        public static bool IsBetween(this ConsistentHash thisHash, ConsistentHash start, ConsistentHash end)
        {
            if (start < end)
            {
                if (thisHash >= start && thisHash < end)
                {
                    return true;
                }
            }
            else //wraparound
            {
                if (thisHash >= start || thisHash < end)
                {
                    return true;
                }
            }

            return false;
        }
    }
}