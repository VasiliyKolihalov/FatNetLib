using Kolyhalov.UdpFramework.Microtypes;

namespace Kolyhalov.UdpFramework.Configurations;

public class ServerConfiguration : Configuration
{
    // todo: think about setting reasonable defaults or getting rid of this property at all
    public Count MaxPeers { get; }

    public ServerConfiguration(Port port, string connectionKey, Count maxPeers, Frequency? framerate,
        TimeSpan? exchangeTimeout) : base(port, connectionKey, framerate, exchangeTimeout)
    {
        MaxPeers = maxPeers ?? throw new UdpFrameworkException("MaxPeers cannot be null");
    }
}