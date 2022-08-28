using Kolyhalov.UdpFramework.Microtypes;

namespace Kolyhalov.UdpFramework.Configurations;

public class ServerConfiguration : Configuration
{
    public Count MaxPeers { get; } // todo: think about getting rid of this feature

    public ServerConfiguration(Port port, string connectionKey, Count maxPeers, Frequency? framerate,
        TimeSpan? exchangeTimeout) : base(port, connectionKey, framerate, exchangeTimeout)
    {
        MaxPeers = maxPeers ?? throw new UdpFrameworkException("MaxPeersCount cannot be null");
    }
}