using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Wrappers;

namespace Kolyhalov.FatNetLib.Modules.DefaultModules.Dependencies;

public class ClientConnectionStarterModule : IModule
{
    public void Setup(ModuleContext moduleContext)
    {
        moduleContext.DependencyContext.Put<IConnectionStarter>(context => new ClientConnectionStarter(
            context.Get<INetManager>(),
            context.Get<ClientConfiguration>(),
            context.Get<IProtocolVersionProvider>()));
    }
}