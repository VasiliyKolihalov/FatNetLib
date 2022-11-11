using System.Collections.Generic;
using Kolyhalov.FatNetLib.Wrappers;
using LiteNetLib;

namespace Kolyhalov.FatNetLib.Subscribers
{
    public class ClientPeerDisconnectedEventSubscriber : IPeerDisconnectedEventSubscriber
    {
        private readonly IList<INetPeer> _connectedPeers;

        public ClientPeerDisconnectedEventSubscriber(IList<INetPeer> connectedPeers)
        {
            _connectedPeers = connectedPeers;
        }

        public void Handle(INetPeer peer, DisconnectInfo info)
        {
            _connectedPeers.Remove(peer);
        }
    }
}
