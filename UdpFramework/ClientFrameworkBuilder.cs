using LiteNetLib;
using Microsoft.Extensions.Logging;

namespace Kolyhalov.UdpFramework;

public class ClientFrameworkBuilder
{
    public string Address { get; init; } = null!;
    public Port Port { get; init; } = null!;
    public Framerate Framerate { get; set; } = null!;
    public ILogger? Logger { get; init; }

    public ClientUdpFramework Build()
    {
        var clientConfiguration = new ClientConfiguration(
            Address,
            Port,
            connectionKey: string.Empty, //Todo version control based on connection string
            Framerate);

        return new ClientUdpFramework(clientConfiguration,
            Logger,
            new EndpointsStorage(),
            new EndpointsInvoker(),
            new EventBasedNetListener());
    }
}