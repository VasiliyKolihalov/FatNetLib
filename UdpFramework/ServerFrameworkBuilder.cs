using LiteNetLib;
using Microsoft.Extensions.Logging;

namespace Kolyhalov.UdpFramework;

public class ServerFrameworkBuilder
{
    public Port Port { get; init; } = null!;
    public Framerate Framerate { get; init; } = null!;
    public Count MaxPeersCount { get; init; } = null!;
    public ILogger? Logger { get; init; }

    public ServerUdpFramework Build()
    {
        var serverConfiguration = new ServerConfiguration(
            Port,
            connectionKey: string.Empty, //Todo version control based on connection string
            Framerate,
            MaxPeersCount);

        return new ServerUdpFramework(serverConfiguration,
            Logger,
            new EndpointsStorage(),
            new EndpointsInvoker(),
            new EventBasedNetListener());
    }
}