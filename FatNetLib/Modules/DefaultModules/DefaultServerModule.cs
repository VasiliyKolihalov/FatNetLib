using Kolyhalov.FatNetLib.Modules.DefaultModules.Dependencies;

namespace Kolyhalov.FatNetLib.Modules.DefaultModules;

public class DefaultServerModule : IServerBuildTypeModule
{
    public void Setup(ModuleContext moduleContext)
    {
        new CommonDependenciesModule().Setup(moduleContext);
        new DefaultJsonSerializerModule().Setup(moduleContext);
        new ServerSubscribersModule().Setup(moduleContext);
        new ServerConnectionStarterModule().Setup(moduleContext);
        new ServerInitialEndpointsModule().Setup(moduleContext);
        new NetEventListenerModule().Setup(moduleContext);
        new DefaultServerConfigurationValuesModule().Setup(moduleContext);
    }
}