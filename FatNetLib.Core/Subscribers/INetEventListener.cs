namespace Kolyhalov.FatNetLib.Core.Subscribers
{
    public interface INetEventListener
    {
        public void Run();

        public void Stop();
    }
}
