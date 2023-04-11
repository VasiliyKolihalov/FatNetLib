using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Attributes;
using Kolyhalov.FatNetLib.Core.Controllers;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Wrappers;
using static Kolyhalov.FatNetLib.Core.Constants.RouteConstants.Strings.Event;

namespace Kolyhalov.FatNetLib.Core.Subscribers.Client
{
    public class ClientPeerDisconnectedEventController : IController
    {
        private readonly IList<INetPeer> _connectedPeers;

        public ClientPeerDisconnectedEventController(IList<INetPeer> connectedPeers)
        {
            _connectedPeers = connectedPeers;
        }

        [EventListener]
        [Route(PeerDisconnected)]
        public void Handle(Package package)
        {
            var body = package.GetBodyAs<PeerDisconnectedBody>();
            _connectedPeers.Remove(body.Peer);
        }
    }
}
