using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Middlewares;
using Kolyhalov.FatNetLib.Monitors;
using Kolyhalov.FatNetLib.Subscribers;
using Kolyhalov.FatNetLib.Wrappers;
using ILoggerProvider = Kolyhalov.FatNetLib.Loggers.ILoggerProvider;

namespace Kolyhalov.FatNetLib.Modules.DefaultModules.Dependencies;

public class ServerSubscribersModule : IModule
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

        dependencyContext.Put<IPeerConnectedEventSubscriber>(new ServerPeerConnectedEventSubscriber(
            dependencyContext.Get<IList<INetPeer>>("ConnectedPeers")));

        dependencyContext.Put<IConnectionRequestEventSubscriber>(new ServerConnectionRequestEventSubscriber(
            dependencyContext.Get<ServerConfiguration>(),
            dependencyContext.Get<INetManager>(),
            dependencyContext.Get<IProtocolVersionProvider>(),
            dependencyContext.Get<ILoggerProvider>()));

        dependencyContext.Put<IPeerDisconnectedEventSubscriber>(new ServerPeerDisconnectedEventSubscriber(
            dependencyContext.Get<IList<INetPeer>>("ConnectedPeers"),
            dependencyContext.Get<IEndpointsStorage>()));
    }
}