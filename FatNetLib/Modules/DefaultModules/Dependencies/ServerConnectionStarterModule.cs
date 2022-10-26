using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Wrappers;

namespace Kolyhalov.FatNetLib.Modules.DefaultModules.Dependencies;

public class ServerConnectionStarterModule : IModule
{
    public void Setup(ModuleContext moduleContext)
    {
        moduleContext.DependencyContext.Put<IConnectionStarter>(context => new ServerConnectionStarter(
            context.Get<INetManager>(),
            context.Get<Configuration>()));
    }
}