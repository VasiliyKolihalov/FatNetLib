using Microsoft.Extensions.Logging;

namespace Kolyhalov.UdpFramework;

public class ServerBuilderOptions
{
    public Port Port { get; init; } = null!;
    public Framerate Framerate { get; init; } = null!;
    public int MaxPeersCount { get; init; }
    public ILogger? Logger { get; set; }

    public void Validate()
    {
        if (Port == null)
            throw new UdpFrameworkException("Port cannot be null");

        if (Framerate == null)
            throw new UdpFrameworkException("Framerate cannot be null");

        if (MaxPeersCount is 0 or < 0)
            throw new UdpFrameworkException("MaxPeersCount cannot be zero or bellow");
    }
}