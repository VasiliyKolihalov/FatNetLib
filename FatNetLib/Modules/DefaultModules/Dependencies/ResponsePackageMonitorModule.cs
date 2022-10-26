using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Monitors;
using Monitor = Kolyhalov.FatNetLib.Wrappers.Monitor;

namespace Kolyhalov.FatNetLib.Modules.DefaultModules.Dependencies;

public class ResponsePackageMonitorModule : IModule
{
    public void Setup(ModuleContext moduleContext)
    {
        moduleContext.DependencyContext.Put<IResponsePackageMonitor>(context => new ResponsePackageMonitor(
            new Monitor(),
            context.Get<Configuration>(),
            context.Get<IResponsePackageMonitorStorage>()));
    }
}