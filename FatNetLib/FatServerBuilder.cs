using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Microtypes;
using Kolyhalov.FatNetLib.Middlewares;
using Kolyhalov.FatNetLib.NetPeers;
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
    public IList<IMiddleware> SendingMiddlewares { get; init; } = new List<IMiddleware>();
    public IList<IMiddleware> ReceivingMiddlewares { get; init; } = new List<IMiddleware>();

    public FatNetLib Build()
    {
        var configuration = new ServerConfiguration(
            Port,
            connectionKey: string.Empty, //Todo protocol version control instead of connection key
            MaxPeers,
            Framerate,
            ExchangeTimeout);

        var endpointsStorage = new EndpointsStorage();
        var endpointsInvoker = new EndpointsInvoker();
        var connectedPeers = new List<INetPeer>();
        var listener = new EventBasedNetListener();
        var monitor = new ResponsePackageMonitor(new Monitor(), configuration.ExchangeTimeout,
            new ResponsePackageMonitorStorage());

        var receivingMiddlewaresRunner = new MiddlewaresRunner(SendingMiddlewares);
        var sendingMiddlewaresRunner = new MiddlewaresRunner(ReceivingMiddlewares);

        var packageHandler = new PackageHandler(endpointsStorage,
            endpointsInvoker,
            receivingMiddlewaresRunner,
            sendingMiddlewaresRunner,
            connectedPeers);

        var receiverHandlerEvent = new NetworkReceiveEventHandler(packageHandler, monitor);

        var packageListener = new ServerListener(listener,
            receiverHandlerEvent,
            new NetManager(listener),
            connectedPeers,
            endpointsStorage,
            Logger,
            configuration);

        var client = new Client(connectedPeers, endpointsStorage, monitor, sendingMiddlewaresRunner, receivingMiddlewaresRunner);
        var endpointsRecorder = new EndpointRecorder(endpointsStorage);

        return new FatNetLib(client, endpointsRecorder, packageListener);
    }
}