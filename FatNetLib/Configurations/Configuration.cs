using Kolyhalov.FatNetLib.Microtypes;

namespace Kolyhalov.FatNetLib.Configurations;

public abstract class Configuration
{
    public Port? Port { get; set; }

    public Frequency? Framerate { get; set; }

    public TimeSpan? ExchangeTimeout { get; set; }

    public virtual void Patch(Configuration patch)
    {
        if (patch.Port is not null)
            Port = patch.Port;

        if (patch.Framerate is not null)
            Framerate = patch.Framerate;

        if (patch.ExchangeTimeout is not null)
            ExchangeTimeout = patch.ExchangeTimeout;
    }
}
