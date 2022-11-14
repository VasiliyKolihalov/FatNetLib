using System.Collections.Generic;
using System.Threading.Tasks;
using Kolyhalov.FatNetLib.Core.Initializers;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Wrappers;
using static Kolyhalov.FatNetLib.Core.Utils.ExceptionUtils;

namespace Kolyhalov.FatNetLib.Core.Subscribers
{
    public class ClientPeerConnectedEventSubscriber : IPeerConnectedEventSubscriber
    {
        private readonly IList<INetPeer> _connectedPeers;
        private readonly IInitialEndpointsRunner _initialEndpointsRunner;
        private readonly ILogger _logger;

        public ClientPeerConnectedEventSubscriber(
            IList<INetPeer> connectedPeers,
            IInitialEndpointsRunner initialEndpointsRunner,
            ILogger logger)
        {
            _connectedPeers = connectedPeers;
            _initialEndpointsRunner = initialEndpointsRunner;
            _logger = logger;
        }

        public void Handle(INetPeer peer)
        {
            _connectedPeers.Add(peer);

            Task.Run(() =>
                CatchExceptionsTo(_logger, @try: () =>
                    _initialEndpointsRunner.Run()));
        }
    }
}
