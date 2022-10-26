using Kolyhalov.FatNetLib.Endpoints;

namespace Kolyhalov.FatNetLib.Modules.DefaultModules.Dependencies;

public class EndpointsInvokerModule : IModule
{
    public void Setup(ModuleContext moduleContext)
    {
        moduleContext.DependencyContext.Put<IEndpointsInvoker>(_ => new EndpointsInvoker());
    }
}