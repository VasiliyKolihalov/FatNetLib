using Microsoft.Extensions.Logging;

namespace Kolyhalov.UdpFramework;

public class ClientBuilderOptions
{
    public string Address { get; init; } = null!;
    public Port Port { get; init; } = null!;
    public Framerate Framerate { get; set; } = null!;
    public ILogger? Logger { get; set; }

    public void Validate()
    {
        if (Address == null)
            throw new UdpFrameworkException("Address cannot be null");
        
        if (Port == null)
            throw new UdpFrameworkException("Port cannot be null");

        if (Framerate == null)
            throw new UdpFrameworkException("Framerate cannot be null");
    }
}