using Kolyhalov.FatNetLib.Microtypes;

namespace Kolyhalov.FatNetLib.Configurations;

public class ServerConfiguration : Configuration
{
    public Count? MaxPeers { get; set; }

    public override void Patch(Configuration other)
    {
        base.Patch(other);
        if (other is ServerConfiguration serverConfiguration && serverConfiguration.MaxPeers != null)
            MaxPeers = serverConfiguration.MaxPeers;
    }
}