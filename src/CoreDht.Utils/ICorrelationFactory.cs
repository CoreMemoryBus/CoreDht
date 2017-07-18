namespace CoreDht.Utils
{
    public interface ICorrelationFactory<out T>
    {
        T GetNextCorrelation();
    }
}