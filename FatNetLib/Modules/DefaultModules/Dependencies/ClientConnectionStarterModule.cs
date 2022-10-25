using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Wrappers;

namespace Kolyhalov.FatNetLib.Modules.DefaultModules.Dependencies;

public class ClientConnectionStarterModule : IModule
{
    public void Setup(ModuleContext moduleContext)
    {
        IDependencyContext dependencyContext = moduleContext.DependencyContext;
        dependencyContext.Put<IConnectionStarter>(new ClientConnectionStarter(
            dependencyContext.Get<INetManager>(),
            dependencyContext.Get<ClientConfiguration>(),
            dependencyContext.Get<IProtocolVersionProvider>()));
    }
}