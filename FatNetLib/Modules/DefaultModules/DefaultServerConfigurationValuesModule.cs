using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Microtypes;

namespace Kolyhalov.FatNetLib.Modules.DefaultModules;

public class DefaultServerConfigurationValuesModule : IModule
{
    private static readonly Count DefaultMaxPeers = new(int.MaxValue);

    public void Setup(ModuleContext moduleContext)
    {
        new DefaultConfigurationValuesModule().Setup(moduleContext);
        (moduleContext.Configuration as ServerConfiguration)!.MaxPeers ??= DefaultMaxPeers;
    }
}