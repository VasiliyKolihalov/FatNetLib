using LiteNetLib;
using Microsoft.Extensions.Logging;

namespace Kolyhalov.UdpFramework;

public class ServerFrameworkBuilder
{
    public Port Port { get; init; } = null!;
    public Framerate Framerate { get; init; } = null!;
    public int MaxPeersCount { get; init; }
    public ILogger? Logger { get; set; }

    public ServerUdpFramework Build()
    {
        if (Port == null)
            throw new UdpFrameworkException("Port cannot be null");

        if (Framerate == null)
            throw new UdpFrameworkException("Framerate cannot be null");

        if (MaxPeersCount is 0 or < 0)
            throw new UdpFrameworkException("MaxPeersCount cannot be zero or bellow");

        if (Logger == null)
        {
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });
            Logger = loggerFactory.CreateLogger<UdpFramework>();
        }

        var serverConfiguration = new ServerConfiguration(
            Port.UdpPort,
            string.Empty, //Todo control version
            Framerate.ServerFramerate,
            Framerate.ServerFramerate);

        return new ServerUdpFramework(serverConfiguration,
            Logger,
            new EndpointsStorage(),
            new EndpointsInvoker(),
            new EventBasedNetListener());
    }
}