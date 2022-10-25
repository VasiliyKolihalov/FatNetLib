using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Middlewares;
using Kolyhalov.FatNetLib.Monitors;
using Kolyhalov.FatNetLib.Wrappers;

namespace Kolyhalov.FatNetLib.Modules.DefaultModules.Dependencies;

public class ClientModule : IModule
{
    public void Setup(ModuleContext moduleContext)
    {
        IDependencyContext dependencyContext = moduleContext.DependencyContext;
        dependencyContext.Put<IClient>(new Client(dependencyContext.Get<IList<INetPeer>>("ConnectedPeers"),
            dependencyContext.Get<IEndpointsStorage>(),
            dependencyContext.Get<IResponsePackageMonitor>(),
            dependencyContext.Get<IMiddlewaresRunner>("SendingMiddlewaresRunner")));
    }
}