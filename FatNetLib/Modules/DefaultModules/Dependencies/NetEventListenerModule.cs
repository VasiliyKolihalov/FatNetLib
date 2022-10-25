using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Subscribers;
using Kolyhalov.FatNetLib.Wrappers;
using LiteNetLib;
using ILoggerProvider = Kolyhalov.FatNetLib.Loggers.ILoggerProvider;

namespace Kolyhalov.FatNetLib.Modules.DefaultModules.Dependencies;

public class NetEventListenerModule : IModule
{
    public void Setup(ModuleContext moduleContext)
    {
        IDependencyContext dependencyContext = moduleContext.DependencyContext;
        dependencyContext.Put(new NetEventListener(dependencyContext.Get<EventBasedNetListener>(),
            dependencyContext.Get<INetworkReceiveEventSubscriber>(),
            dependencyContext.Get<IPeerConnectedEventSubscriber>(),
            dependencyContext.Get<IConnectionRequestEventSubscriber>(),
            dependencyContext.Get<IPeerDisconnectedEventSubscriber>(),
            dependencyContext.Get<INetManager>(),
            dependencyContext.Get<IConnectionStarter>(),
            dependencyContext.Get<Configuration>(),
            dependencyContext.Get<ILoggerProvider>()));
    }
}