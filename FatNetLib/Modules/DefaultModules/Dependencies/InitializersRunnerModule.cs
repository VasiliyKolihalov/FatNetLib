using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Initializers;

namespace Kolyhalov.FatNetLib.Modules.DefaultModules.Dependencies;

public class InitializersRunnerModule : IModule
{
    public void Setup(ModuleContext moduleContext)
    {
        IDependencyContext dependencyContext = moduleContext.DependencyContext;
        dependencyContext.Put<IInitialEndpointsRunner>(new InitialEndpointsRunner(dependencyContext.Get<IClient>(),
            dependencyContext.Get<IEndpointsStorage>(),
            dependencyContext));
    }
}