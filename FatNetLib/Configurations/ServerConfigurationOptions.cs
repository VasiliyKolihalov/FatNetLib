using Kolyhalov.FatNetLib.Microtypes;

namespace Kolyhalov.FatNetLib.Configurations;

public class ServerConfigurationOptions : ConfigurationOptions
{
    public Count? MaxPeers { get; init; }
}