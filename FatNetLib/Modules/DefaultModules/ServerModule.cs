using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Initializers.Controllers.Server;
using Kolyhalov.FatNetLib.Middlewares;
using Kolyhalov.FatNetLib.Monitors;
using Kolyhalov.FatNetLib.Subscribers;
using Kolyhalov.FatNetLib.Wrappers;
using Microsoft.Extensions.Logging;

namespace Kolyhalov.FatNetLib.Modules.DefaultModules;

public class ServerModule : Module
{
    private IDependencyContext _dependencyContext = null!;

    public override void Setup(ModuleContext moduleContext)
    {
        _dependencyContext = moduleContext.DependencyContext;
        ChildModules.Add(new CommonModule());
        ChildModules.Add(new DefaultServerConfigurationValuesModule());
        CreateConfiguration();
        CreateSubscribers();
        CreateConnectionStarter();
        CreateInitialEndpoints(moduleContext);
    }

    private void CreateConfiguration()
    {
        if (!_dependencyContext.IsExist<Configuration>())
        {
            Configuration configuration = new ServerConfiguration();
            _dependencyContext.Put(_ => configuration);
        }

        if (_dependencyContext.Get<Configuration>() is not ServerConfiguration)
            throw new FatNetLibException(
                "Wrong type configuration was registered in modules. Should be ServerConfiguration");

        _dependencyContext.CopyReference(typeof(Configuration), typeof(ServerConfiguration));
    }

    private void CreateSubscribers()
    {
        _dependencyContext.Put<INetworkReceiveEventSubscriber>(context => new NetworkReceiveEventSubscriber(
            context.Get<IResponsePackageMonitor>(),
            context.Get<IMiddlewaresRunner>("ReceivingMiddlewaresRunner"),
            context.Get<PackageSchema>("DefaultPackageSchema"),
            context,
            context.Get<IEndpointsStorage>(),
            context.Get<IEndpointsInvoker>(),
            context.Get<IMiddlewaresRunner>("SendingMiddlewaresRunner"),
            context.Get<IList<INetPeer>>("ConnectedPeers")));

        _dependencyContext.Put<IPeerConnectedEventSubscriber>(context => new ServerPeerConnectedEventSubscriber(
            context.Get<IList<INetPeer>>("ConnectedPeers")));

        _dependencyContext.Put<IConnectionRequestEventSubscriber>(context => new ServerConnectionRequestEventSubscriber(
            context.Get<ServerConfiguration>().MaxPeers!,
            context.Get<INetManager>(),
            context.Get<IProtocolVersionProvider>(),
            context.Get<ILogger>()));

        _dependencyContext.Put<IPeerDisconnectedEventSubscriber>(context => new ServerPeerDisconnectedEventSubscriber(
            context.Get<IList<INetPeer>>("ConnectedPeers"),
            context.Get<IEndpointsStorage>()));
    }

    private void CreateConnectionStarter()
    {
        _dependencyContext.Put<IConnectionStarter>(context => new ServerConnectionStarter(
            context.Get<INetManager>(),
            context.Get<Configuration>().Port!));
    }

    private static void CreateInitialEndpoints(ModuleContext moduleContext)
    {
        var exchangeEndpointsController = new ExchangeEndpointsController(moduleContext.EndpointsStorage);
        var initializationController = new InitializationController(moduleContext.EndpointsStorage);

        moduleContext.EndpointRecorder.AddController(exchangeEndpointsController);
        moduleContext.EndpointRecorder.AddController(initializationController);
    }
}