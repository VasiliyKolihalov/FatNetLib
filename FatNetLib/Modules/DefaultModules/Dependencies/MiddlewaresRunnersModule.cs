using Kolyhalov.FatNetLib.Middlewares;

namespace Kolyhalov.FatNetLib.Modules.DefaultModules.Dependencies;

public class MiddlewaresRunnersModule : IModule
{
    public void Setup(ModuleContext moduleContext)
    {
        IDependencyContext dependencyContext = moduleContext.DependencyContext;
        dependencyContext.Put("SendingMiddlewaresRunner",
            new MiddlewaresRunner(dependencyContext.Get<IList<IMiddleware>>("SendingMiddlewares")));
        dependencyContext.Put("ReceivingMiddlewaresRunner",
            new MiddlewaresRunner(dependencyContext.Get<IList<IMiddleware>>("ReceivingMiddlewares")));
    }
}