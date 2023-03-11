namespace Kolyhalov.FatNetLib.Core.Wrappers
{
    public class NetManager : INetManager
    {
        private readonly LiteNetLib.NetManager _netManager;

        public int ConnectedPeersCount => _netManager.ConnectedPeersCount;

        public NetManager(LiteNetLib.NetManager netManager)
        {
            _netManager = netManager;
        }

        public INetPeer Connect(string address, int port, string key)
        {
            return new NetPeer(_netManager.Connect(address, port, key));
        }

        public bool Start()
        {
            return _netManager.Start();
        }

        public bool Start(int port)
        {
            return _netManager.Start(port);
        }

        public void Stop()
        {
            _netManager.Stop();
        }

        public void PollEvents()
        {
            _netManager.PollEvents();
        }
    }
}
