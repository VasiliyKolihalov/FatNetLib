using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Middlewares;
using Kolyhalov.FatNetLib.Monitors;
using Kolyhalov.FatNetLib.Wrappers;

namespace Kolyhalov.FatNetLib.Modules.DefaultModules.Dependencies;

public class ClientModule : IModule
{
    public void Setup(ModuleContext moduleContext)
    {
        moduleContext.DependencyContext.Put<IClient>(context => new Client(
            context.Get<IList<INetPeer>>("ConnectedPeers"),
            context.Get<IEndpointsStorage>(),
            context.Get<IResponsePackageMonitor>(),
            context.Get<IMiddlewaresRunner>("SendingMiddlewaresRunner")));
    }
}