using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Initializers;
using Kolyhalov.FatNetLib.Initializers.Controllers.Client;
using Kolyhalov.FatNetLib.Middlewares;
using Kolyhalov.FatNetLib.Monitors;
using Kolyhalov.FatNetLib.Subscribers;
using Kolyhalov.FatNetLib.Wrappers;
using Microsoft.Extensions.Logging;

namespace Kolyhalov.FatNetLib.Modules.DefaultModules;

public class ClientModule : Module
{
    private IDependencyContext _dependencyContext = null!;

    public override void Setup(ModuleContext moduleContext)
    {
        _dependencyContext = moduleContext.DependencyContext;
        ChildModules.Add(new CommonModule());
        ChildModules.Add(new DefaultClientConfigurationValuesModule());
        CreateConfiguration();
        CreateInitializersRunner();
        CreateSubscribers();
        CreateConnectionStarter();
        CreateInitialEndpoints(moduleContext);
    }

    private void CreateConfiguration()
    {
        if (!_dependencyContext.IsExist<Configuration>())
        {
            Configuration configuration = new ClientConfiguration();
            _dependencyContext.Put(_ => configuration);
        }
        if (_dependencyContext.Get<Configuration>() is not ClientConfiguration)
            throw new FatNetLibException(
                "Wrong type configuration was registered in modules. Should be ClientConfiguration");
        
        _dependencyContext.CopyReference(typeof(Configuration), typeof(ClientConfiguration));
    }

    private void CreateInitializersRunner()
    {
        _dependencyContext.Put<IInitialEndpointsRunner>(context => new InitialEndpointsRunner(
            context.Get<IClient>(),
            context.Get<IEndpointsStorage>(),
            context));
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

        _dependencyContext.Put<IPeerConnectedEventSubscriber>(context => new ClientPeerConnectedEventSubscriber(
            context.Get<IList<INetPeer>>("ConnectedPeers"),
            context.Get<IInitialEndpointsRunner>(),
            context.Get<ILogger>()));

        _dependencyContext.Put<IConnectionRequestEventSubscriber>(_ => new ClientConnectionRequestEventSubscriber());

        _dependencyContext.Put<IPeerDisconnectedEventSubscriber>(context => new ClientPeerDisconnectedEventSubscriber(
            context.Get<IList<INetPeer>>("ConnectedPeers")));
    }

    private void CreateConnectionStarter()
    {
        _dependencyContext.Put<IConnectionStarter>(context => new ClientConnectionStarter(
            context.Get<INetManager>(),
            context.Get<ClientConfiguration>().Address!,
            context.Get<ClientConfiguration>().Port!,
            context.Get<IProtocolVersionProvider>()));
    }

    private static void CreateInitialEndpoints(ModuleContext moduleContext)
    {
        var controller = new ExchangeEndpointsController(moduleContext.EndpointsStorage);
        moduleContext.EndpointRecorder.AddController(controller);
    }
}