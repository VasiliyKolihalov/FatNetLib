using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Providers;
using Kolyhalov.FatNetLib.Core.Wrappers;

namespace Kolyhalov.FatNetLib.Core.Subscribers.Server
{
    public class ServerConnectionRequestEventSubscriber : IConnectionRequestEventSubscriber
    {
        private readonly ServerConfiguration _configuration;
        private readonly INetManager _netManager;
        private readonly string _protocolVersion;
        private readonly ILogger _logger;

        public ServerConnectionRequestEventSubscriber(
            ServerConfiguration configuration,
            INetManager netManager,
            IProtocolVersionProvider protocolVersionProvider,
            ILogger logger)
        {
            _configuration = configuration;
            _netManager = netManager;
            _protocolVersion = protocolVersionProvider.Get();
            _logger = logger;
        }

        public void Handle(IConnectionRequest request)
        {
            if (_netManager.ConnectedPeersCount >= _configuration.MaxPeers!.Value)
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
