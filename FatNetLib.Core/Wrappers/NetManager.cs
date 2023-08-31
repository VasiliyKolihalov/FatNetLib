using Kolyhalov.FatNetLib.Core.Storages;

namespace Kolyhalov.FatNetLib.Core.Wrappers
{
    public class NetManager : INetManager
    {
        private readonly LiteNetLib.NetManager _netManager;
        private readonly IIdProvider _idProvider;

        public int ConnectedPeersCount => _netManager.ConnectedPeersCount;

        public NetManager(LiteNetLib.NetManager netManager, IIdProvider idProvider)
        {
            _netManager = netManager;
            _idProvider = idProvider;
        }

        public INetPeer Connect(string address, int port, string key)
        {
            LiteNetLib.NetPeer peer = _netManager.Connect(address, port, key);
            return new NetPeer(peer, _idProvider.GetId(peer));
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
