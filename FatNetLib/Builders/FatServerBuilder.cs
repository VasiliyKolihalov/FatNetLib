using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Microtypes;
using Kolyhalov.FatNetLib.Middlewares;
using Kolyhalov.FatNetLib.NetPeers;
using Kolyhalov.FatNetLib.ResponsePackageMonitors;
using LiteNetLib;

namespace Kolyhalov.FatNetLib.Builders;

public class FatServerBuilder : FatNetLibBuilder
{
    public Count? MaxPeers { get; init; }
    public Frequency? Framerate { get; init; }
  

    public override FatNetLib Build()
    {
        var configuration = new ServerConfiguration(
            Port,
            connectionKey: string.Empty, //Todo protocol version control instead of connection key
            MaxPeers,
            Framerate,
            ExchangeTimeout);

        var endpointsStorage = Endpoints.EndpointsStorage;
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

        return new FatNetLib(client, packageListener);
    }
}