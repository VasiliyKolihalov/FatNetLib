using Kolyhalov.FatNetLib.Initializers;
using Kolyhalov.FatNetLib.Loggers;
using Kolyhalov.FatNetLib.Wrappers;
using static Kolyhalov.FatNetLib.Utils.ExceptionUtils;

namespace Kolyhalov.FatNetLib.Subscribers;

public class ClientPeerConnectedEventSubscriber : IPeerConnectedEventSubscriber
{
    private readonly IList<INetPeer> _connectedPeers;
    private readonly IInitialEndpointsRunner _initialEndpointsRunner;
    private readonly ILoggerProvider _loggerProvider;

    public ClientPeerConnectedEventSubscriber(IList<INetPeer> connectedPeers,
        IInitialEndpointsRunner initialEndpointsRunner,
        ILoggerProvider loggerProvider)
    {
        _connectedPeers = connectedPeers;
        _initialEndpointsRunner = initialEndpointsRunner;
        _loggerProvider = loggerProvider;
    }

    public void Handle(INetPeer peer)
    {
        _connectedPeers.Add(peer);

        Task.Run(() =>
            CatchExceptionsTo(_loggerProvider.Logger,
                @try: () =>
                    _initialEndpointsRunner.Run()));
    }
}