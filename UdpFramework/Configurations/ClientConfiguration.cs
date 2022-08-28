using Kolyhalov.UdpFramework.Microtypes;

namespace Kolyhalov.UdpFramework.Configurations;

public class ClientConfiguration : Configuration
{
    public string Address { get; }

    public ClientConfiguration(string address, Port port, string connectionKey, Frequency? framerate,
        TimeSpan? exchangeTimeout) : base(port, connectionKey, framerate, exchangeTimeout)
    {
        Address = address ?? throw new UdpFrameworkException("Address cannot be null");
    }
}