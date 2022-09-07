using Kolyhalov.UdpFramework.Configurations;
using Kolyhalov.UdpFramework.Endpoints;
using Kolyhalov.UdpFramework.Microtypes;
using Kolyhalov.UdpFramework.ResponsePackageMonitors;
using LiteNetLib;
using Microsoft.Extensions.Logging;

namespace Kolyhalov.UdpFramework;

public class ServerFrameworkBuilder
{
    public Port Port { get; init; } = null!;
    public Count? MaxPeers { get; init; }
    public Frequency? Framerate { get; init; }
    public ILogger? Logger { get; init; }
    public TimeSpan? ExchangeTimeout { get; init; }

    public ServerUdpFramework Build()
    {
        var configuration = new ServerConfiguration(
            Port,
            connectionKey: string.Empty, //Todo protocol version control instead of connection key
            MaxPeers,
            Framerate,
            ExchangeTimeout);

        return new ServerUdpFramework(configuration,
            Logger,
            new EndpointsStorage(),
            new EndpointsInvoker(),
            new EventBasedNetListener(),
            new ResponsePackageMonitor(new Monitor(), configuration.ExchangeTimeout,
                new ResponsePackageMonitorStorage()));
    }
}