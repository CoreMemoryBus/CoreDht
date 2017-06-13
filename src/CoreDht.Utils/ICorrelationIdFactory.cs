namespace CoreDht.Utils
{
    public interface ICorrelationIdFactory
    {
        CorrelationId GetNextCorrelation();
    }
}