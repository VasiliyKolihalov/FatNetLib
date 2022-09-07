using Kolyhalov.UdpFramework.Microtypes;

namespace Kolyhalov.UdpFramework.Configurations;

public abstract class Configuration
{
    public Port Port { get; }
    public string ConnectionKey { get; }
    public Frequency Framerate { get; }
    public TimeSpan ExchangeTimeout { get; }

    private static readonly Frequency DefaultFramerate = new (60);
    private static readonly TimeSpan DefaultExchangeTimeout = TimeSpan.FromMinutes(1);

    protected Configuration(Port port, string connectionKey, Frequency? framerate, TimeSpan? exchangeTimeout)
    {
        Port = port ?? throw new UdpFrameworkException("Port cannot be null");
        ConnectionKey = connectionKey ?? throw new UdpFrameworkException("Connection key cannot be null");
        Framerate = framerate ?? DefaultFramerate;
        ExchangeTimeout = exchangeTimeout ?? DefaultExchangeTimeout;
    }
}