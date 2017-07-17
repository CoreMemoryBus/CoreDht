namespace CoreDht.Utils
{
    public class ActionTimerFactory : IActionTimerFactory
    {
        public IActionTimer CreateTimer()
        {
            return new ActionTimer();
        }
    }
}