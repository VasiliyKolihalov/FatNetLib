using Kolyhalov.UdpFramework.Microtypes;

namespace Kolyhalov.UdpFramework.Configurations;

public abstract class Configuration
{
    public Port Port { get; }
    public string ConnectionKey { get; }
    public Frequency Framerate { get; }
    public TimeSpan ExchangeTimeout { get; }

    protected Configuration(Port port, string connectionKey, Frequency? framerate, TimeSpan? exchangeTimeout)
    {
        Port = port ?? throw new UdpFrameworkException("Port cannot be null");
        ConnectionKey = connectionKey ?? throw new UdpFrameworkException("Connection key cannot be null");
        Framerate = framerate ?? new Frequency(60);
        ExchangeTimeout = exchangeTimeout ?? TimeSpan.FromMinutes(1);
    }
}