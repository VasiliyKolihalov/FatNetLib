using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Microtypes;
using Kolyhalov.FatNetLib.ResponsePackageMonitors;
using LiteNetLib;
using Microsoft.Extensions.Logging;

namespace Kolyhalov.FatNetLib;

public class FatServerBuilder
{
    public Port Port { get; init; } = null!;
    public Count? MaxPeers { get; init; }
    public Frequency? Framerate { get; init; }
    public ILogger? Logger { get; init; }
    public TimeSpan? ExchangeTimeout { get; init; }

    public ServerFatNetLib Build()
    {
        var configuration = new ServerConfiguration(
            Port,
            connectionKey: string.Empty, //Todo protocol version control instead of connection key
            MaxPeers,
            Framerate,
            ExchangeTimeout);

        return new ServerFatNetLib(configuration,
            Logger,
            new EndpointsStorage(),
            new EndpointsInvoker(),
            new EventBasedNetListener(),
            new ResponsePackageMonitor(new Monitor(), configuration.ExchangeTimeout,
                new ResponsePackageMonitorStorage()));
    }
}