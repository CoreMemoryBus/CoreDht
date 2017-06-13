namespace CoreDht.Utils
{
    public interface IRandomNumberGenerator
    {
        int Next();
        int Next(int minValue, int maxValue);
    }
}