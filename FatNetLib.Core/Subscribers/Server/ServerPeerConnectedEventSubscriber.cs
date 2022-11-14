using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Wrappers;

namespace Kolyhalov.FatNetLib.Core.Subscribers.Server
{
    public class ServerPeerConnectedEventSubscriber : IPeerConnectedEventSubscriber
    {
        private readonly IList<INetPeer> _connectedPeers;

        public ServerPeerConnectedEventSubscriber(IList<INetPeer> connectedPeers)
        {
            _connectedPeers = connectedPeers;
        }

        public void Handle(INetPeer peer)
        {
            _connectedPeers.Add(peer);
        }
    }
}
