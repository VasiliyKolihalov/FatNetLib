using LiteNetLib;

namespace Kolyhalov.FatNetLib.Modules.DefaultModules.Dependencies;

public class EventBasedNetListenerModule : IModule
{
    public void Setup(ModuleContext moduleContext)
    {
        moduleContext.DependencyContext.Put(new EventBasedNetListener());
    }
}