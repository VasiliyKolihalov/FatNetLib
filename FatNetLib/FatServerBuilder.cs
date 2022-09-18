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

        var packageHandler = new PackageHandler(endpointsStorage,
            endpointsInvoker,
            receivingMiddlewaresRunner: new MiddlewaresRunner(SendingMiddlewares),
            sendingMiddlewaresRunner: new MiddlewaresRunner(ReceivingMiddlewares),
            connectedPeers);

        var packageListener = new ServerListener(listener, 
            new NetManager(listener), 
            packageHandler, 
            connectedPeers,
            endpointsStorage, 
            monitor, 
            Logger, 
            configuration);

        var client = new FatClient(connectedPeers, endpointsStorage, monitor);
        var endpointsRecorder = new EndpointRecorder(endpointsStorage);

        return new FatNetLib(client, endpointsRecorder, packageListener);
    }
}