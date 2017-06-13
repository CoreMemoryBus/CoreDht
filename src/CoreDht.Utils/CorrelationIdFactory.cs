namespace CoreDht.Utils
{
    public class CorrelationIdFactory : ICorrelationIdFactory
    {
        public CorrelationId GetNextCorrelation()
        {
            return CorrelationId.NewId();
        }
    }
}