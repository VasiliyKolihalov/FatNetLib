using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Attributes;
using Kolyhalov.FatNetLib.Core.Controllers;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Wrappers;
using static Kolyhalov.FatNetLib.Core.Constants.RouteConstants.Strings.Events;

namespace Kolyhalov.FatNetLib.Core.Subscribers.Server
{
    public class ServerPeerDisconnectedEventController : IController
    {
        private readonly IList<INetPeer> _connectedPeers;
        private readonly IEndpointsStorage _endpointsStorage;

        public ServerPeerDisconnectedEventController(
            IList<INetPeer> connectedPeers,
            IEndpointsStorage endpointsStorage)
        {
            _connectedPeers = connectedPeers;
            _endpointsStorage = endpointsStorage;
        }

        [EventListener]
        [Route(PeerDisconnected)]
        public void Handle(Package package)
        {
            INetPeer peer = package.GetBodyAs<PeerDisconnectedBody>().Peer;
            _connectedPeers.Remove(peer);
            _endpointsStorage.RemoteEndpoints.Remove(peer.Id);
        }
    }
}
