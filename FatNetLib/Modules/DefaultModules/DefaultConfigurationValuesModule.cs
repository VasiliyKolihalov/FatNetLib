using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Microtypes;

namespace Kolyhalov.FatNetLib.Modules.DefaultModules;

public class DefaultConfigurationValuesModule : IModule
{
    private static readonly Port DefaultPort = new(2812);
    private static readonly Frequency DefaultFramerate = new(60);
    private static readonly TimeSpan DefaultExchangeTimeout = TimeSpan.FromMinutes(1);

    public void Setup(ModuleContext moduleContext)
    {
        Configuration configuration = moduleContext.Configuration;
        configuration.Port ??= DefaultPort;
        configuration.Framerate ??= DefaultFramerate;
        configuration.ExchangeTimeout ??= DefaultExchangeTimeout;
    }
}