namespace Kolyhalov.FatNetLib.Modules.DefaultModules.Dependencies;

public class ProtocolVersionProviderModule : IModule
{
    public void Setup(ModuleContext moduleContext)
    {
        moduleContext.DependencyContext.Put<IProtocolVersionProvider>(_ => new ProtocolVersionProvider());
    }
}