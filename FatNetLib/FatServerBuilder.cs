using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Initializers.Controllers.Server;
using Kolyhalov.FatNetLib.Microtypes;
using Kolyhalov.FatNetLib.Middlewares;
using Kolyhalov.FatNetLib.Monitors;
using Kolyhalov.FatNetLib.Subscribers;
using Kolyhalov.FatNetLib.Wrappers;
using LiteNetLib;

namespace Kolyhalov.FatNetLib;

public class FatServerBuilder : FatNetLibBuilder
{
    public Count? MaxPeers { get; init; }

    public override FatNetLib Build()
    {
        CreateCommonDependencies();
        CreateConfiguration();
        CreateResponsePackageMonitor();
        CreateClient();
        CreateSubscribers();
        CreateServerListener();
        RegisterInitialEndpoints();
        return CreateFatNetLib();
    }

    private void RegisterInitialEndpoints()
    {
        var endpointsStorage = Context.Get<IEndpointsStorage>();
        var exchangeEndpointsController = new ExchangeEndpointsController(endpointsStorage,
            Context.Get<IClient>(),
            JsonSerializer);

        var initializationController = new InitializationController(
            endpointsStorage,
            JsonSerializer);

        var endpointRecorder = Context.Get<IEndpointRecorder>();
        endpointRecorder.AddController(exchangeEndpointsController);
        endpointRecorder.AddController(initializationController);
    }


    private void CreateConfiguration()
    {
        Context.Put(new ServerConfiguration(Port, MaxPeers, Framerate, ExchangeTimeout));
        Context.CopyReference(typeof(ServerConfiguration), typeof(Configuration));
    }

    private void CreateSubscribers()
    {
        Context.Put<INetworkReceiveEventSubscriber>(new NetworkReceiveEventSubscriber(
            Context.Get<IPackageHandler>(),
            Context.Get<IResponsePackageMonitor>(),
            Context.Get<IMiddlewaresRunner>("ReceivingMiddlewaresRunner"),
            Context.Get<PackageSchema>("DefaultPackageSchema"),
            Context));

        Context.Put<IConnectionRequestEventSubscriber>(new ConnectionRequestEventSubscriber(
            Context.Get<ServerConfiguration>(),
            Context.Get<INetManager>(),
            Context.Get<IProtocolVersionProvider>(),
            Logger));

        Context.Put<IPeerConnectedEventSubscriber>(new ServerPeerConnectedEventSubscriber(
            Context.Get<IList<INetPeer>>("ConnectedPeers")));
    }

    private void CreateServerListener()
    {
        Context.Put<NetEventListener>(new ServerListener(Context.Get<EventBasedNetListener>(),
            Context.Get<INetworkReceiveEventSubscriber>(),
            Context.Get<IPeerConnectedEventSubscriber>(),
            Context.Get<INetManager>(),
            Context.Get<IList<INetPeer>>("ConnectedPeers"),
            Context.Get<IEndpointsStorage>(),
            Logger,
            Context.Get<Configuration>(),
            Context.Get<IConnectionRequestEventSubscriber>()));
    }
}