using System.Collections.Generic;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Wrappers;
using LiteNetLib;

namespace Kolyhalov.FatNetLib.Subscribers
{
    public class ServerPeerDisconnectedEventSubscriber : IPeerDisconnectedEventSubscriber
    {
        private readonly IList<INetPeer> _connectedPeers;
        private readonly IEndpointsStorage _endpointsStorage;

        public ServerPeerDisconnectedEventSubscriber(IList<INetPeer> connectedPeers, IEndpointsStorage endpointsStorage)
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
