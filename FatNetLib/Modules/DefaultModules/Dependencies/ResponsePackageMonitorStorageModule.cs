using Kolyhalov.FatNetLib.Monitors;

namespace Kolyhalov.FatNetLib.Modules.DefaultModules.Dependencies;

public class ResponsePackageMonitorStorageModule : IModule
{
    public void Setup(ModuleContext moduleContext)
    {
        moduleContext.DependencyContext.Put<IResponsePackageMonitorStorage>(_ => new ResponsePackageMonitorStorage());
    }
}