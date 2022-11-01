using Kolyhalov.FatNetLib.Microtypes;

namespace Kolyhalov.FatNetLib.Configurations;

public abstract class Configuration
{
    public Port? Port { get; set; }
    public Frequency? Framerate { get; set; }
    public TimeSpan? ExchangeTimeout { get; set; }

    public virtual void Patch(Configuration patch)
    {
        if (patch.Port != null)
            Port = patch.Port;

        if (patch.Framerate != null)
            Framerate = patch.Framerate;

        if (patch.ExchangeTimeout != null)
            ExchangeTimeout = patch.ExchangeTimeout;
    }
}