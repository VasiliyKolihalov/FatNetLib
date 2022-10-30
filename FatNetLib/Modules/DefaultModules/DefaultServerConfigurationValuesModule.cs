using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Microtypes;

namespace Kolyhalov.FatNetLib.Modules.DefaultModules;

public class DefaultServerConfigurationValuesModule : Module
{
    public override void Setup(ModuleContext moduleContext)
    {
        var defaultConfiguration = new ServerConfiguration
        {
            Port = new Port(2812),
            Framerate = new Frequency(60),
            ExchangeTimeout = TimeSpan.FromMinutes(1),
            MaxPeers = new Count(int.MaxValue)
        };
        var configuration = (moduleContext.Configuration as ServerConfiguration)!;
        configuration.Port ??= defaultConfiguration.Port;
        configuration.Framerate ??= defaultConfiguration.Framerate;
        configuration.ExchangeTimeout ??= defaultConfiguration.ExchangeTimeout;
        configuration.MaxPeers = defaultConfiguration.MaxPeers;
    }
}