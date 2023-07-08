using System.Collections.Generic;
using System.Threading.Tasks;
using Kolyhalov.FatNetLib.Core.Attributes;
using Kolyhalov.FatNetLib.Core.Controllers;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Runners;
using Kolyhalov.FatNetLib.Core.Wrappers;
using static Kolyhalov.FatNetLib.Core.Constants.RouteConstants.Strings.Events;

namespace Kolyhalov.FatNetLib.Core.Subscribers.Client
{
    public class ClientPeerConnectedEventController : IController
    {
        private readonly IList<INetPeer> _connectedPeers;
        private readonly IInitializersRunner _initializersRunner;

        public ClientPeerConnectedEventController(
            IList<INetPeer> connectedPeers,
            IInitializersRunner initializersRunner)
        {
            _connectedPeers = connectedPeers;
            _initializersRunner = initializersRunner;
        }

        [EventListener]
        [Route(PeerConnected)]
        public async Task HandleAsync(Package package)
        {
            var peer = package.GetBodyAs<INetPeer>();
            await HandleAsync(peer);
        }

        private async Task HandleAsync(INetPeer peer)
        {
            _connectedPeers.Add(peer);

            await _initializersRunner.RunAsync();
        }
    }
}
