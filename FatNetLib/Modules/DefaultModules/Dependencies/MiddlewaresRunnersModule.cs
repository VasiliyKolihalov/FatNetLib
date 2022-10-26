using Kolyhalov.FatNetLib.Middlewares;

namespace Kolyhalov.FatNetLib.Modules.DefaultModules.Dependencies;

public class MiddlewaresRunnersModule : IModule
{
    public void Setup(ModuleContext moduleContext)
    {
        IDependencyContext dependencyContext = moduleContext.DependencyContext;
        dependencyContext.Put("SendingMiddlewaresRunner",
            context => new MiddlewaresRunner(context.Get<IList<IMiddleware>>("SendingMiddlewares")));
        dependencyContext.Put("ReceivingMiddlewaresRunner",
            context => new MiddlewaresRunner(context.Get<IList<IMiddleware>>("ReceivingMiddlewares")));
    }
}