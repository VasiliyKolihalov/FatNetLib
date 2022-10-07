using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Subscribers;
using Kolyhalov.FatNetLib.Wrappers;
using LiteNetLib;
using Microsoft.Extensions.Logging;

namespace Kolyhalov.FatNetLib;

// Todo: replace inheritance with aggregation
public class ClientListener : NetEventListener
{
    private readonly string _protocolVersion;
    protected override ClientConfiguration Configuration { get; }

    public ClientListener(EventBasedNetListener listener,
        INetworkReceiveEventSubscriber receiverEventSubscriber,
        IPeerConnectedEventSubscriber peerConnectedEventSubscriber,
        INetManager netManager,
        IList<INetPeer> connectedPeers,
        IEndpointsStorage endpointsStorage,
        ILogger? logger,
        ClientConfiguration configuration,
        IProtocolVersionProvider protocolVersionProvider) : base(listener,
        receiverEventSubscriber,
        peerConnectedEventSubscriber,
        netManager,
        connectedPeers,
        endpointsStorage,
        logger)
    {
        Configuration = configuration;
        _protocolVersion = protocolVersionProvider.Get();
    }

    protected override void StartListen()
    {
        NetManager.Start();
        NetManager.Connect(Configuration.Address,
            Configuration.Port.Value,
            key: _protocolVersion);
    }
}