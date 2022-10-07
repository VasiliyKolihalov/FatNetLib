using Kolyhalov.FatNetLib.Initializers;
using Kolyhalov.FatNetLib.Wrappers;
using Microsoft.Extensions.Logging;
using static Kolyhalov.FatNetLib.Utils.ExceptionUtils;

namespace Kolyhalov.FatNetLib.Subscribers;

public class ClientPeerConnectedEventSubscriber : IPeerConnectedEventSubscriber
{
    private readonly IList<INetPeer> _connectedPeers;
    private readonly IInitialConfigurationEndpointsRunner _initialConfigurationEndpointsRunner;
    private readonly ILogger? _logger;

    public ClientPeerConnectedEventSubscriber(IList<INetPeer> connectedPeers,
        IInitialConfigurationEndpointsRunner initialConfigurationEndpointsRunner,
        ILogger? logger)
    {
        _connectedPeers = connectedPeers;
        _initialConfigurationEndpointsRunner = initialConfigurationEndpointsRunner;
        _logger = logger;
    }

    public void Handle(INetPeer peer)
    {
        _connectedPeers.Add(peer);
        
        Task.Run(() =>
            CatchExceptionsTo(_logger,
                @try: () =>
                    _initialConfigurationEndpointsRunner.Run()));
    }
}