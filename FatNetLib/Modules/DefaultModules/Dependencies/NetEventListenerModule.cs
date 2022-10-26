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
        moduleContext.DependencyContext.Put(context => new NetEventListener(context.Get<EventBasedNetListener>(),
            context.Get<INetworkReceiveEventSubscriber>(),
            context.Get<IPeerConnectedEventSubscriber>(),
            context.Get<IConnectionRequestEventSubscriber>(),
            context.Get<IPeerDisconnectedEventSubscriber>(),
            context.Get<INetManager>(),
            context.Get<IConnectionStarter>(),
            context.Get<Configuration>(),
            context.Get<ILoggerProvider>()));
    }
}