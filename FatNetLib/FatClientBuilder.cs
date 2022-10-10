using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.InitialControllers.Client;
using Kolyhalov.FatNetLib.Initializers;
using Kolyhalov.FatNetLib.Middlewares;
using Kolyhalov.FatNetLib.Monitors;
using Kolyhalov.FatNetLib.Subscribers;
using Kolyhalov.FatNetLib.Wrappers;
using LiteNetLib;

namespace Kolyhalov.FatNetLib;

public class FatClientBuilder : FatNetLibBuilder
{
    public string Address { get; init; } = null!;

    public override FatNetLib Build()
    {
        CreateCommonDependencies();
        CreateConfiguration();
        CreateResponsePackageMonitor();
        CreateClient();
        CreateInitializersRunner();
        CreateSubscribers();
        CreateClientListener();
        RegisterInitialEndpoints();
        return CreateFatNetLib();
    }

    protected override void RegisterInitialEndpoints()
    {
        HoldAndGetEndpointsController controller =
            new HoldAndGetEndpointsController(Context.Get<IEndpointsStorage>(),
                JsonSerializer);

        var endpointRecorder = Context.Get<IEndpointRecorder>();
        endpointRecorder.AddController(controller, isInitial: true);
    }

    private void CreateConfiguration()
    {
        Context.Put(new ClientConfiguration(Address, Port, Framerate, ExchangeTimeout));
        Context.CopyReference(typeof(ClientConfiguration), typeof(Configuration));
    }

    private void CreateInitializersRunner()
    {
        Context.Put<IInitialEndpointsRunner>(new InitialEndpointsRunner(Context.Get<IClient>(),
            Context.Get<IEndpointsStorage>(),
            Context));
    }

    private void CreateSubscribers()
    {
        Context.Put<INetworkReceiveEventSubscriber>(new NetworkReceiveEventSubscriber(
            Context.Get<IPackageHandler>(),
            Context.Get<IResponsePackageMonitor>(),
            Context.Get<IMiddlewaresRunner>("ReceivingMiddlewaresRunner"),
            Context.Get<PackageSchema>("DefaultPackageSchema"),
            Context));

        Context.Put<IPeerConnectedEventSubscriber>(new ClientPeerConnectedEventSubscriber(
            Context.Get<IList<INetPeer>>("ConnectedPeers"),
            Context.Get<IInitialEndpointsRunner>(),
            Logger));
    }

    private void CreateClientListener()
    {
        Context.Put<NetEventListener>(new ClientListener(Context.Get<EventBasedNetListener>(),
            Context.Get<INetworkReceiveEventSubscriber>(),
            Context.Get<IPeerConnectedEventSubscriber>(),
            Context.Get<INetManager>(),
            Context.Get<IList<INetPeer>>("ConnectedPeers"),
            Context.Get<IEndpointsStorage>(),
            Logger,
            Context.Get<ClientConfiguration>(),
            Context.Get<IProtocolVersionProvider>()));
    }
}