using Kolyhalov.FatNetLib.Wrappers;
using LiteNetLib;
using NetManager = LiteNetLib.NetManager;

namespace Kolyhalov.FatNetLib.Modules.DefaultModules.Dependencies;

public class NetManagerModule : IModule
{
    public void Setup(ModuleContext moduleContext)
    {
        moduleContext.DependencyContext.Put<INetManager>(context =>
            new Wrappers.NetManager(new NetManager(context.Get<EventBasedNetListener>())));
    }
}