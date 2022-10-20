using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Initializers.Controllers.Server;
using Kolyhalov.FatNetLib.Microtypes;
using Kolyhalov.FatNetLib.Middlewares;
using Kolyhalov.FatNetLib.Monitors;
using Kolyhalov.FatNetLib.Subscribers;
using Kolyhalov.FatNetLib.Wrappers;
using Microsoft.Extensions.Logging;

namespace Kolyhalov.FatNetLib;

public class FatServerBuilder : FatNetLibBuilder
{
    public FatServerBuilder(ServerConfigurationOptions configurationOptions) : base(configurationOptions)
    {
    }

    public override FatNetLib Build()
    {
        CreateConfiguration();
        CreateResponsePackageMonitor();
        CreateClient();
        CreateSubscribers();
        CreateConnectionStarter();
        CreateNetEventListener();
        RegisterInitialEndpoints();
        return CreateFatNetLib();
    }

    private void RegisterInitialEndpoints()
    {
        var endpointsStorage = Context.Get<IEndpointsStorage>();
        var exchangeEndpointsController = new ExchangeEndpointsController(endpointsStorage);
        var initializationController = new InitializationController(endpointsStorage);

        var endpointRecorder = Context.Get<IEndpointRecorder>();
        endpointRecorder.AddController(exchangeEndpointsController);
        endpointRecorder.AddController(initializationController);
    }

    private void CreateConfiguration()
    {
        var builderConfigurationOptions = BuilderConfigurationOptions as ServerConfigurationOptions;
        var moduleConfigurationOptions = Context.Get<ConfigurationOptions>("ModuleConfigurationOptions");
        
        ILogger? logger = builderConfigurationOptions!.Logger ?? moduleConfigurationOptions.Logger;
        Context.Put(logger);
        
        Port? port = builderConfigurationOptions.Port ?? moduleConfigurationOptions.Port;
        Frequency? framerate = builderConfigurationOptions.Framerate ?? moduleConfigurationOptions.Framerate;
        TimeSpan? exchangeTimeout = builderConfigurationOptions.ExchangeTimeout ??
                                    moduleConfigurationOptions.ExchangeTimeout;
        Context.Put(new ServerConfiguration(port!, builderConfigurationOptions.MaxPeers, framerate, exchangeTimeout));
        Context.CopyReference(typeof(ServerConfiguration), typeof(Configuration));
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

        Context.Put<IPeerConnectedEventSubscriber>(new ServerPeerConnectedEventSubscriber(
            Context.Get<IList<INetPeer>>("ConnectedPeers")));

        Context.Put<IConnectionRequestEventSubscriber>(new ServerConnectionRequestEventSubscriber(
            Context.Get<ServerConfiguration>(),
            Context.Get<INetManager>(),
            Context.Get<IProtocolVersionProvider>(),
            Context.Get<ILogger>()));

        Context.Put<IPeerDisconnectedEventSubscriber>(new ServerPeerDisconnectedEventSubscriber(
            Context.Get<IList<INetPeer>>("ConnectedPeers"),
            Context.Get<IEndpointsStorage>()));
    }

    private void CreateConnectionStarter()
    {
        Context.Put<IConnectionStarter>(new ServerConnectionStarter(
            Context.Get<INetManager>(),
            Context.Get<Configuration>()));
    }
}