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
        dependencyContext.Put<INetworkReceiveEventSubscriber>(new NetworkReceiveEventSubscriber(
            dependencyContext.Get<IResponsePackageMonitor>(),
            dependencyContext.Get<IMiddlewaresRunner>("ReceivingMiddlewaresRunner"),
            dependencyContext.Get<PackageSchema>("DefaultPackageSchema"),
            dependencyContext,
            dependencyContext.Get<IEndpointsStorage>(),
            dependencyContext.Get<IEndpointsInvoker>(),
            dependencyContext.Get<IMiddlewaresRunner>("SendingMiddlewaresRunner"),
            dependencyContext.Get<IList<INetPeer>>("ConnectedPeers")));

        dependencyContext.Put<IPeerConnectedEventSubscriber>(new ClientPeerConnectedEventSubscriber(
            dependencyContext.Get<IList<INetPeer>>("ConnectedPeers"),
            dependencyContext.Get<IInitialEndpointsRunner>(),
            dependencyContext.Get<ILoggerProvider>()));

        dependencyContext.Put<IConnectionRequestEventSubscriber>(new ClientConnectionRequestEventSubscriber());

        dependencyContext.Put<IPeerDisconnectedEventSubscriber>(new ClientPeerDisconnectedEventSubscriber(
            dependencyContext.Get<IList<INetPeer>>("ConnectedPeers")));
    }
}