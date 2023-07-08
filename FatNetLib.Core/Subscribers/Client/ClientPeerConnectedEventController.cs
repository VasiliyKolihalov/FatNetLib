using System.Collections.Generic;
using System.Threading.Tasks;
using Kolyhalov.FatNetLib.Core.Attributes;
using Kolyhalov.FatNetLib.Core.Controllers;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Runners;
using Kolyhalov.FatNetLib.Core.Wrappers;
using static Kolyhalov.FatNetLib.Core.Constants.RouteConstants.Strings.Events;
using static Kolyhalov.FatNetLib.Core.Utils.ExceptionUtils;

namespace Kolyhalov.FatNetLib.Core.Subscribers.Client
{
    public class ClientPeerConnectedEventController : IController
    {
        private readonly IList<INetPeer> _connectedPeers;
        private readonly IInitializersRunner _initializersRunner;
        private readonly ILogger _logger;

        public ClientPeerConnectedEventController(
            IList<INetPeer> connectedPeers,
            IInitializersRunner initializersRunner,
            ILogger logger)
        {
            _connectedPeers = connectedPeers;
            _initializersRunner = initializersRunner;
            _logger = logger;
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

            await CatchExceptionsToAsync(_logger, @try: async () =>
                await _initializersRunner.RunAsync());
        }
    }
}
