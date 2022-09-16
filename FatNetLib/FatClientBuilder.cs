using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Microtypes;
using Kolyhalov.FatNetLib.Middlewares;
using Kolyhalov.FatNetLib.ResponsePackageMonitors;
using LiteNetLib;
using Microsoft.Extensions.Logging;

namespace Kolyhalov.FatNetLib;

public class FatClientBuilder
{
    public string Address { get; init; } = null!;
    public Port Port { get; init; } = null!;
    public Frequency? Framerate { get; init; }
    public ILogger? Logger { get; init; }
    public TimeSpan? ExchangeTimeout { get; init; }
    public IList<IMiddleware> SendingMiddlewares { get; init; } = new List<IMiddleware>();
    public IList<IMiddleware> ReceivingMiddlewares { get; init; } = new List<IMiddleware>();

    public ClientFatNetLib Build()
    {
        var configuration = new ClientConfiguration(
            Address,
            Port,
            connectionKey: string.Empty, //Todo protocol version control instead of connection key
            Framerate,
            ExchangeTimeout);

        return new ClientFatNetLib(configuration,
            Logger,
            new EndpointsStorage(),
            new EndpointsInvoker(),
            new EventBasedNetListener(),
            new ResponsePackageMonitor(new Monitor(),
                configuration.ExchangeTimeout,
                new ResponsePackageMonitorStorage()),
            sendingMiddlewaresRunner: new MiddlewaresRunner(SendingMiddlewares),
            receivingMiddlewaresRunner: new MiddlewaresRunner(ReceivingMiddlewares));
    }
}