using Kolyhalov.FatNetLib.Microtypes;

namespace Kolyhalov.FatNetLib.Configurations;

public class ServerConfiguration : Configuration
{
    public Count MaxPeers { get; }
    private static readonly Count DefaultMaxPeers = new(int.MaxValue);

    public ServerConfiguration(Port port, 
        Count? maxPeers, 
        Frequency? framerate,
        TimeSpan? exchangeTimeout) : base(port, framerate, exchangeTimeout)
    {
        MaxPeers = maxPeers ?? DefaultMaxPeers;
    }
}