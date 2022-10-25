using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Monitors;
using Monitor = Kolyhalov.FatNetLib.Wrappers.Monitor;

namespace Kolyhalov.FatNetLib.Modules.DefaultModules.Dependencies;

public class ResponsePackageMonitorModule : IModule
{
    public void Setup(ModuleContext moduleContext)
    {
        IDependencyContext dependencyContext = moduleContext.DependencyContext;
        dependencyContext.Put<IResponsePackageMonitor>(new ResponsePackageMonitor(new Monitor(),
            dependencyContext.Get<Configuration>(),
            dependencyContext.Get<IResponsePackageMonitorStorage>()));
    }
}