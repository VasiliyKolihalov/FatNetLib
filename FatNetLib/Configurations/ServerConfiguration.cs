using Kolyhalov.FatNetLib.Microtypes;

namespace Kolyhalov.FatNetLib.Configurations;

public class ServerConfiguration : Configuration
{
    public Count? MaxPeers { get; set; }
}