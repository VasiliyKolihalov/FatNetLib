using Kolyhalov.UdpFramework.Microtypes;

namespace Kolyhalov.UdpFramework.Configurations;

public class ServerConfiguration : Configuration
{
    public Count MaxPeers { get; }
    private static readonly Count DefaultMaxPeers = new(int.MaxValue);

    public ServerConfiguration(Port port, 
        string connectionKey, 
        Count? maxPeers, 
        Frequency? framerate,
        TimeSpan? exchangeTimeout) : base(port, connectionKey, framerate, exchangeTimeout)
    {
        MaxPeers = maxPeers ?? DefaultMaxPeers;
    }
}