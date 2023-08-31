using Kolyhalov.FatNetLib.Core.Storages;

namespace Kolyhalov.FatNetLib.Core.Wrappers
{
    public class NetManager : INetManager
    {
        private readonly LiteNetLib.NetManager _netManager;
        private readonly IIdStorage _idStorage;

        public int ConnectedPeersCount => _netManager.ConnectedPeersCount;

        public NetManager(LiteNetLib.NetManager netManager, IIdStorage idStorage)
        {
            _netManager = netManager;
            _idStorage = idStorage;
        }

        public INetPeer Connect(string address, int port, string key)
        {
            LiteNetLib.NetPeer peer = _netManager.Connect(address, port, key);
            return new NetPeer(peer, _idStorage.GetId(peer));
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
