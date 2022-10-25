using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Wrappers;

namespace Kolyhalov.FatNetLib.Modules.DefaultModules.Dependencies;

public class ServerConnectionStarterModule : IModule
{
    public void Setup(ModuleContext moduleContext)
    {
        IDependencyContext dependencyContext = moduleContext.DependencyContext;
        dependencyContext.Put<IConnectionStarter>(new ServerConnectionStarter(
            dependencyContext.Get<INetManager>(),
            dependencyContext.Get<Configuration>()));
    }
}