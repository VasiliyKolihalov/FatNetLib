using Kolyhalov.FatNetLib.Modules.DefaultModules.Dependencies;

namespace Kolyhalov.FatNetLib.Modules.DefaultModules;

public class DefaultClientModule : IClientBuildTypeModule
{
    public void Setup(ModuleContext moduleContext)
    {
        new CommonDependenciesModule().Setup(moduleContext);
        new DefaultJsonSerializerModule().Setup(moduleContext);
        new InitializersRunnerModule().Setup(moduleContext);
        new ClientSubscribersModule().Setup(moduleContext);
        new ClientConnectionStarterModule().Setup(moduleContext);
        new ClientInitialEndpointsModule().Setup(moduleContext);
        new NetEventListenerModule().Setup(moduleContext);
        new DefaultClientConfigurationValuesModule().Setup(moduleContext);
    }
}