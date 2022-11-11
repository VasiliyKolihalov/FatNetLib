using System.Collections.Generic;
using System.Threading.Tasks;
using Kolyhalov.FatNetLib.Initializers;
using Kolyhalov.FatNetLib.Loggers;
using Kolyhalov.FatNetLib.Wrappers;
using static Kolyhalov.FatNetLib.Utils.ExceptionUtils;

namespace Kolyhalov.FatNetLib.Subscribers
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
