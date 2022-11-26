using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Wrappers;
using LiteNetLib;

namespace Kolyhalov.FatNetLib.Core.Subscribers.Server
{
    public class ServerPeerDisconnectedEventSubscriber : IPeerDisconnectedEventSubscriber
    {
        private readonly IList<INetPeer> _connectedPeers;
        private readonly IEndpointsStorage _endpointsStorage;

        public ServerPeerDisconnectedEventSubscriber(
            IList<INetPeer> connectedPeers,
            IEndpointsStorage endpointsStorage)
        {
            _connectedPeers = connectedPeers;
            _endpointsStorage = endpointsStorage;
        }

        public void Handle(INetPeer peer, DisconnectInfo info)
        {
            _connectedPeers.Remove(peer);
            _endpointsStorage.RemoteEndpoints.Remove(peer.Id);
        }
    }
}
