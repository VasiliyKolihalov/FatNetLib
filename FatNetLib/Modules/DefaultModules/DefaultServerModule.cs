using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Initializers.Controllers.Server;
using Kolyhalov.FatNetLib.Microtypes;
using Kolyhalov.FatNetLib.Subscribers;
using Kolyhalov.FatNetLib.Wrappers;
using Microsoft.Extensions.Logging;

namespace Kolyhalov.FatNetLib.Modules.DefaultModules;

public class DefaultServerModule : IModule
{
    private IDependencyContext _dependencyContext = null!;

    public void Setup(ModuleContext moduleContext)
    {
        _dependencyContext = moduleContext.DependencyContext;
        CreateConfiguration();
        CreateSubscribers();
        CreateConnectionStarter();
        CreateInitialEndpoints(moduleContext);
    }

    public IList<IModule> ChildModules { get; } = new List<IModule> { new DefaultCommonModule() };

    private void CreateConfiguration()
    {
        var defaultConfiguration = new ServerConfiguration
        {
            Port = new Port(2812),
            Framerate = new Frequency(60),
            ExchangeTimeout = TimeSpan.FromSeconds(10),
            MaxPeers = new Count(int.MaxValue)
        };
        _dependencyContext.Put<Configuration>(_ => defaultConfiguration);
        _dependencyContext.CopyReference(typeof(Configuration), typeof(ServerConfiguration));
    }

    private void CreateSubscribers()
    {
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
