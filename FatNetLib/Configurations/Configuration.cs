using Kolyhalov.FatNetLib.Microtypes;

namespace Kolyhalov.FatNetLib.Configurations;

public abstract class Configuration
{
    public Port Port { get; }
    public Frequency Framerate { get; }
    public TimeSpan ExchangeTimeout { get; }

    private static readonly Frequency DefaultFramerate = new (60);
    private static readonly TimeSpan DefaultExchangeTimeout = TimeSpan.FromMinutes(1);

    protected Configuration(Port port, Frequency? framerate, TimeSpan? exchangeTimeout)
    {
        Port = port ?? throw new FatNetLibException("Port cannot be null");
        Framerate = framerate ?? DefaultFramerate;
        ExchangeTimeout = exchangeTimeout ?? DefaultExchangeTimeout;
    }
}