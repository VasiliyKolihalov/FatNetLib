using Kolyhalov.UdpFramework.Configurations;
using Kolyhalov.UdpFramework.Endpoints;
using Kolyhalov.UdpFramework.Microtypes;
using Kolyhalov.UdpFramework.ResponsePackageMonitors;
using LiteNetLib;
using Microsoft.Extensions.Logging;

namespace Kolyhalov.UdpFramework;

public class ClientFrameworkBuilder
{
    public string Address { get; init; } = null!;
    public Port Port { get; init; } = null!;
    public Frequency? Framerate { get; init; }
    public ILogger? Logger { get; init; }
    public TimeSpan? ExchangeTimeout { get; init; }

    public ClientUdpFramework Build()
    {
        var configuration = new ClientConfiguration(
            Address,
            Port,
            connectionKey: string.Empty, //Todo protocol version control instead of connection key
            Framerate,
            ExchangeTimeout);

        return new ClientUdpFramework(configuration,
            Logger,
            new EndpointsStorage(),
            new EndpointsInvoker(),
            new EventBasedNetListener(),
            new ResponsePackageMonitor(new Monitor(), configuration, new ResponsePackageMonitorStorage()));
    }
}