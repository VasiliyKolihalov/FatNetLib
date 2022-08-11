using LiteNetLib;
using Microsoft.Extensions.Logging;

namespace Kolyhalov.UdpFramework;

public class ClientFrameworkBuilder
{
    public string Address { get; init; } = null!;
    public Port Port { get; init; } = null!;
    public Framerate Framerate { get; set; } = null!;
    public ILogger? Logger { get; set; }

    public ClientUdpFramework Build()
    {
        if (Address == null)
            throw new UdpFrameworkException("Address cannot be null");

        if (Port == null)
            throw new UdpFrameworkException("Port cannot be null");

        if (Framerate == null)
            throw new UdpFrameworkException("Framerate cannot be null");

        if (Logger == null)
        {
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });
            Logger = loggerFactory.CreateLogger<UdpFramework>();
        }

        var clientConfiguration = new ClientConfiguration(
            Address,
            Port.UdpPort,
            string.Empty, //Todo control version 
            Framerate.ServerFramerate);

        return new ClientUdpFramework(clientConfiguration,
            Logger,
            new EndpointsStorage(),
            new EndpointsInvoker(),
            new EventBasedNetListener());
    }
}