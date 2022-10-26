using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Initializers;
using Kolyhalov.FatNetLib.Middlewares;
using Kolyhalov.FatNetLib.Monitors;
using Kolyhalov.FatNetLib.Subscribers;
using Kolyhalov.FatNetLib.Wrappers;
using ILoggerProvider = Kolyhalov.FatNetLib.Loggers.ILoggerProvider;

namespace Kolyhalov.FatNetLib.Modules.DefaultModules.Dependencies;

public class ClientSubscribersModule : IModule
{
    public void Setup(ModuleContext moduleContext)
    {
        IDependencyContext dependencyContext = moduleContext.DependencyContext;
        dependencyContext.Put<INetworkReceiveEventSubscriber>(context => new NetworkReceiveEventSubscriber(
            context.Get<IResponsePackageMonitor>(),
            context.Get<IMiddlewaresRunner>("ReceivingMiddlewaresRunner"),
            context.Get<PackageSchema>("DefaultPackageSchema"),
            context,
            context.Get<IEndpointsStorage>(),
            context.Get<IEndpointsInvoker>(),
            context.Get<IMiddlewaresRunner>("SendingMiddlewaresRunner"),
            context.Get<IList<INetPeer>>("ConnectedPeers")));

        dependencyContext.Put<IPeerConnectedEventSubscriber>(context => new ClientPeerConnectedEventSubscriber(
            context.Get<IList<INetPeer>>("ConnectedPeers"),
            context.Get<IInitialEndpointsRunner>(),
            context.Get<ILoggerProvider>()));

        dependencyContext.Put<IConnectionRequestEventSubscriber>(_ => new ClientConnectionRequestEventSubscriber());

        dependencyContext.Put<IPeerDisconnectedEventSubscriber>(context => new ClientPeerDisconnectedEventSubscriber(
            context.Get<IList<INetPeer>>("ConnectedPeers")));
    }
}