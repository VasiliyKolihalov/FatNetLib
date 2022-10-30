using Kolyhalov.FatNetLib.Microtypes;

namespace Kolyhalov.FatNetLib.Configurations;

public abstract class Configuration
{
    public Port? Port { get; set; }
    public Frequency? Framerate { get; set; }
    public TimeSpan? ExchangeTimeout { get; set; }

    public virtual void Patch(Configuration other)
    {
        if (other.Port != null)
            Port = other.Port;

        if (other.Framerate != null)
            Framerate = other.Framerate;

        if (other.ExchangeTimeout != null)
            ExchangeTimeout = other.ExchangeTimeout;
    }
}