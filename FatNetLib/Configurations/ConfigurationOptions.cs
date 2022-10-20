using Kolyhalov.FatNetLib.Microtypes;
using Microsoft.Extensions.Logging;

namespace Kolyhalov.FatNetLib.Configurations;

public class ConfigurationOptions
{
    public Port? Port { get; set; }
    public Frequency? Framerate { get; set; }
    public TimeSpan? ExchangeTimeout { get; set; }
    public ILogger? Logger { get; set; }
}