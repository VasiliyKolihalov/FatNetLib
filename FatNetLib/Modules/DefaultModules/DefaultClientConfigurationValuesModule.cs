using Kolyhalov.FatNetLib.Configurations;

namespace Kolyhalov.FatNetLib.Modules.DefaultModules;

public class DefaultClientConfigurationValuesModule : IModule
{
    private const string DefaultAddress = "localhost";
    public void Setup(ModuleContext moduleContext)
    {
        new DefaultConfigurationValuesModule().Setup(moduleContext);
        (moduleContext.Configuration as ClientConfiguration)!.Address ??= DefaultAddress;
    }
}