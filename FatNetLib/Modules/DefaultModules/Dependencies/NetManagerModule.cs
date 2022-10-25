using Kolyhalov.FatNetLib.Wrappers;
using LiteNetLib;
using NetManager = LiteNetLib.NetManager;

namespace Kolyhalov.FatNetLib.Modules.DefaultModules.Dependencies;

public class NetManagerModule : IModule
{
    public void Setup(ModuleContext moduleContext)
    {
        IDependencyContext dependencyContext = moduleContext.DependencyContext;
        moduleContext.DependencyContext.Put<INetManager>(
            new Wrappers.NetManager(new NetManager(dependencyContext.Get<EventBasedNetListener>())));
    }
}