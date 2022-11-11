using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Wrappers;

namespace Kolyhalov.FatNetLib.Core.Subscribers
{
    public class ServerConnectionRequestEventSubscriber : IConnectionRequestEventSubscriber
    {
        private readonly Count _maxPeers;
        private readonly INetManager _netManager;
        private readonly string _protocolVersion;
        private readonly ILogger _logger;

        public ServerConnectionRequestEventSubscriber(
            Count maxPeers,
            INetManager netManager,
            IProtocolVersionProvider protocolVersionProvider,
            ILogger logger)
        {
            _maxPeers = maxPeers;
            _netManager = netManager;
            _protocolVersion = protocolVersionProvider.Get();
            _logger = logger;
        }

        public void Handle(IConnectionRequest request)
        {
            if (_netManager.ConnectedPeersCount >= _maxPeers.Value)
            {
                _logger.Warn("Connection rejected: Max peers exceeded");
                request.Reject();
                return;
            }

            if (_protocolVersion != request.Data.GetString())
            {
                _logger.Warn("Connection rejected: Protocol version mismatch");
                request.Reject();
                return;
            }

            request.Accept();
        }
    }
}
