namespace Kolyhalov.FatNetLib.Core.Wrappers
{
    public interface INetManager
    {
        public int ConnectedPeersCount { get; }

        public INetPeer? Connect(string address, int port, string key);

        public bool Start();

        public bool Start(int port);

        public void Stop();

        public void PollEvents();
    }
}
