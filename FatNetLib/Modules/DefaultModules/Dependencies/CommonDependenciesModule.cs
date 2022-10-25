namespace Kolyhalov.FatNetLib.Modules.DefaultModules.Dependencies;

public class CommonDependenciesModule : IModule
{
    public void Setup(ModuleContext moduleContext)
    {
        new ResponsePackageMonitorStorageModule().Setup(moduleContext);
        new ConnectedPeersModule().Setup(moduleContext);
        new MiddlewaresRunnersModule().Setup(moduleContext);
        new EndpointsInvokerModule().Setup(moduleContext);
        new EventBasedNetListenerModule().Setup(moduleContext);
        new NetManagerModule().Setup(moduleContext);
        new ProtocolVersionProviderModule().Setup(moduleContext);
        new ResponsePackageMonitorModule().Setup(moduleContext);
        new ClientModule().Setup(moduleContext);
    }
}