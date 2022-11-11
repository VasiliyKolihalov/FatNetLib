namespace Kolyhalov.FatNetLib.Wrappers
{
    public interface INetManager
    {
        public int ConnectedPeersCount { get; }

        public void Connect(string address, int port, string key);

        public void Start();

        public void Start(int port);

        public void Stop();

        public void PollEvents();
    }
}
