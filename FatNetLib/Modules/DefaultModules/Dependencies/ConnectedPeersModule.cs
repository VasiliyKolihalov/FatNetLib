using Kolyhalov.FatNetLib.Wrappers;

namespace Kolyhalov.FatNetLib.Modules.DefaultModules.Dependencies;

public class ConnectedPeersModule : IModule
{
    public void Setup(ModuleContext moduleContext)
    {
        moduleContext.DependencyContext.Put("ConnectedPeers", _ => new List<INetPeer>());
    }
}