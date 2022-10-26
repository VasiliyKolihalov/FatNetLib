using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Initializers;

namespace Kolyhalov.FatNetLib.Modules.DefaultModules.Dependencies;

public class InitializersRunnerModule : IModule
{
    public void Setup(ModuleContext moduleContext)
    {
        moduleContext.DependencyContext.Put<IInitialEndpointsRunner>(context => new InitialEndpointsRunner(
            context.Get<IClient>(),
            context.Get<IEndpointsStorage>(),
            context));
    }
}