using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Initializers;
using Kolyhalov.FatNetLib.Initializers.Controllers.Client;
using Kolyhalov.FatNetLib.Microtypes;
using Kolyhalov.FatNetLib.Middlewares;
using Kolyhalov.FatNetLib.Monitors;
using Kolyhalov.FatNetLib.Subscribers;
using Kolyhalov.FatNetLib.Wrappers;

namespace Kolyhalov.FatNetLib;

public class FatClientBuilder : FatNetLibBuilder
{
    public string Address { get; init; } = null!;

    public override FatNetLib Build()
    {
        CreateConfiguration();
        CreateResponsePackageMonitor();
        CreateClient();
        CreateInitializersRunner();
        CreateSubscribers();
        CreateConnectionStarter();
        CreateNetEventListener();
        RegisterInitialEndpoints();
        return CreateFatNetLib();
    }

    private void RegisterInitialEndpoints()
    {
        var controller = new ExchangeEndpointsController(Context.Get<IEndpointsStorage>());
        var endpointRecorder = Context.Get<IEndpointRecorder>();
        endpointRecorder.AddController(controller);
    }

    private void CreateConfiguration()
    {
        var configurationOptions = Context.Get<ConfigurationOptions>();
        Port port = Port ?? configurationOptions.Port;
        Frequency framerate = Framerate ?? configurationOptions.Framerate;
        TimeSpan exchangeTimeout = ExchangeTimeout ?? configurationOptions.ExchangeTimeout;

        Context.Put(new ClientConfiguration(Address, port, framerate, exchangeTimeout));
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
            Context.Get<IResponsePackageMonitor>(),
            Context.Get<IMiddlewaresRunner>("ReceivingMiddlewaresRunner"),
            Context.Get<PackageSchema>("DefaultPackageSchema"),
            Context,
            Context.Get<IEndpointsStorage>(),
            Context.Get<IEndpointsInvoker>(),
            Context.Get<IMiddlewaresRunner>("SendingMiddlewaresRunner"),
            Context.Get<IList<INetPeer>>("ConnectedPeers")));

        Context.Put<IPeerConnectedEventSubscriber>(new ClientPeerConnectedEventSubscriber(
            Context.Get<IList<INetPeer>>("ConnectedPeers"),
            Context.Get<IInitialEndpointsRunner>(),
            Logger));

        Context.Put<IConnectionRequestEventSubscriber>(new ClientConnectionRequestEventSubscriber());

        Context.Put<IPeerDisconnectedEventSubscriber>(new ClientPeerDisconnectedEventSubscriber(
            Context.Get<IList<INetPeer>>("ConnectedPeers")));
    }

    private void CreateConnectionStarter()
    {
        Context.Put<IConnectionStarter>(new ClientConnectionStarter(
            Context.Get<INetManager>(),
            Context.Get<ClientConfiguration>(),
            Context.Get<IProtocolVersionProvider>()));
    }
}