using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Attributes;
using Kolyhalov.FatNetLib.Core.Controllers;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Wrappers;
using static Kolyhalov.FatNetLib.Core.Constants.RouteConstants.Strings.Events;

namespace Kolyhalov.FatNetLib.Core.Subscribers.Server
{
    public class ServerPeerConnectedEventController : IController
    {
        private readonly IList<INetPeer> _connectedPeers;

        public ServerPeerConnectedEventController(IList<INetPeer> connectedPeers)
        {
            _connectedPeers = connectedPeers;
        }

        [Event]
        [Route(PeerConnected)]
        public void Handle(Package package)
        {
            var peer = package.GetBodyAs<INetPeer>();
            _connectedPeers.Add(peer);
        }
    }
}
