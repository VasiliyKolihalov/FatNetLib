using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Microtypes;

namespace Kolyhalov.FatNetLib.Modules.DefaultModules;

public class DefaultClientConfigurationValuesModule : Module
{
    public override void Setup(ModuleContext moduleContext)
    {
        var defaultConfiguration = new ClientConfiguration
        {
            Port = new Port(2812),
            Framerate = new Frequency(60),
            ExchangeTimeout = TimeSpan.FromMinutes(1),
            Address = "localhost"
        };
        var configuration = (moduleContext.Configuration as ClientConfiguration)!;
        configuration.Port ??= defaultConfiguration.Port;
        configuration.Framerate ??= defaultConfiguration.Framerate;
        configuration.ExchangeTimeout ??= defaultConfiguration.ExchangeTimeout;
        configuration.Address = defaultConfiguration.Address;
    }
}