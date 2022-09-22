using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Microtypes;
using Kolyhalov.FatNetLib.Middlewares;
using Kolyhalov.FatNetLib.ResponsePackageMonitors;
using LiteNetLib;

namespace Kolyhalov.FatNetLib;

public class FatClientBuilder : FatNetLibBuilder
{
    public string Address { get; init; } = null!;
    public Frequency? Framerate { get; init; }


    public override FatNetLib Build()
    {
        var configuration = new ClientConfiguration(
            Address,
            Port,
            connectionKey: string.Empty, //Todo protocol version control instead of connection key
            Framerate,
            ExchangeTimeout);
        
        var monitor = new ResponsePackageMonitor(new Monitor(), configuration.ExchangeTimeout,
            new ResponsePackageMonitorStorage());

        var receivingMiddlewaresRunner = new MiddlewaresRunner(SendingMiddlewares);
        var sendingMiddlewaresRunner = new MiddlewaresRunner(ReceivingMiddlewares);

        var packageHandler = new PackageHandler(EndpointsStorage,
            EndpointsInvoker,
            receivingMiddlewaresRunner,
            sendingMiddlewaresRunner,
            ConnectedPeers);

        var receiverHandlerEvent = new NetworkReceiveEventHandler(packageHandler, monitor);

        var packageListener = new ClientListener(Listener,
            receiverHandlerEvent,
            new NetManager(Listener),
            ConnectedPeers,
            EndpointsStorage,
            Logger,
            configuration);

        var client = new Client(ConnectedPeers, EndpointsStorage, monitor, sendingMiddlewaresRunner,
            receivingMiddlewaresRunner);

        var endpointRecorder = new EndpointRecorder(EndpointsStorage);

        return new FatNetLib(client,endpointRecorder, packageListener);
    }
}