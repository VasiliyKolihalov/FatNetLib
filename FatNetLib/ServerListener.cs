using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Subscribers;
using Kolyhalov.FatNetLib.Wrappers;
using LiteNetLib;
using Microsoft.Extensions.Logging;
using static Kolyhalov.FatNetLib.Utils.ExceptionUtils;
using ConnectionRequest = Kolyhalov.FatNetLib.Wrappers.ConnectionRequest;


namespace Kolyhalov.FatNetLib;

// Todo: replace inheritance with aggregation
public class ServerListener : NetEventListener
{
    private readonly IConnectionRequestEventSubscriber _connectionRequestEventSubscriber;
    protected override Configuration Configuration { get; }

    public ServerListener(EventBasedNetListener listener,
        INetworkReceiveEventSubscriber receiverEventSubscriber,
        IPeerConnectedEventSubscriber peerConnectedEventSubscriber,
        INetManager netManager,
        IList<INetPeer> connectedPeers,
        IEndpointsStorage endpointsStorage,
        ILogger? logger,
        Configuration configuration,
        IConnectionRequestEventSubscriber connectionRequestEventSubscriber) : base(listener,
        receiverEventSubscriber,
        peerConnectedEventSubscriber,
        netManager,
        connectedPeers,
        endpointsStorage,
        logger)
    {
        Configuration = configuration;
        _connectionRequestEventSubscriber = connectionRequestEventSubscriber;
    }

    protected override void StartListen()
    {
        SubscribeOnConnectionRequestEvent();
        SubscribeOnPeerDisconnectedEvent();

        NetManager.Start(Configuration.Port.Value);
    }

    private void SubscribeOnConnectionRequestEvent()
    {
        Listener.ConnectionRequestEvent += request =>
            CatchExceptionsTo(Logger,
                @try: () =>
                    _connectionRequestEventSubscriber.Handle(new ConnectionRequest(request)));
    }

    private void SubscribeOnPeerDisconnectedEvent()
    {
        Listener.PeerDisconnectedEvent += (peer, _) =>
            CatchExceptionsTo(Logger,
                @try: () =>
                    EndpointsStorage.RemoteEndpoints.Remove(peer.Id));
    }
}