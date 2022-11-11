namespace Kolyhalov.FatNetLib.Core
{
    public interface INetEventListener
    {
        public void Run();

        public void Stop();
    }
}
