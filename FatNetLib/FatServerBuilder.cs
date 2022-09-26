using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.LiteNetLibWrappers;
using Kolyhalov.FatNetLib.Microtypes;
using Kolyhalov.FatNetLib.Middlewares;
using Kolyhalov.FatNetLib.ResponsePackageMonitors;
using Kolyhalov.FatNetLib.Subscribers;
using LiteNetLib;
using NetManager = Kolyhalov.FatNetLib.LiteNetLibWrappers.NetManager;

namespace Kolyhalov.FatNetLib;

public class FatServerBuilder : FatNetLibBuilder
{
    public Count? MaxPeers { get; init; }

    public override FatNetLib Build()
    {
        var configuration = new ServerConfiguration(Port, MaxPeers, Framerate, ExchangeTimeout);
        var responsePackageMonitorStorage = new ResponsePackageMonitorStorage();
        var responsePackageMonitor = new ResponsePackageMonitor(new Monitor(),
            configuration.ExchangeTimeout,
            responsePackageMonitorStorage);
        var connectedPeers = new List<INetPeer>();
        var endpointsStorage = new EndpointsStorage();
        var sendingMiddlewaresRunner = new MiddlewaresRunner(SendingMiddlewares);
        var receivingMiddlewaresRunner = new MiddlewaresRunner(ReceivingMiddlewares);
        var client = new Client(connectedPeers, endpointsStorage, responsePackageMonitor, sendingMiddlewaresRunner);

        var endpointRecorder = new EndpointRecorder(endpointsStorage);

        var endpointsInvoker = new EndpointsInvoker();
        var packageHandler = new PackageHandler(endpointsStorage,
            endpointsInvoker,
            sendingMiddlewaresRunner,
            connectedPeers);
        var networkReceiveEventSubscriber = new NetworkReceiveEventSubscriber(packageHandler,
            responsePackageMonitor,
            receivingMiddlewaresRunner,
            DefaultPackageSchema);
        var listener = new EventBasedNetListener();
        var netManager = new NetManager(new LiteNetLib.NetManager(listener));
        var projectVersionProvider = new ProtocolVersionProvider();
        var connectionRequestEventSubscriber =
            new ConnectionRequestEventSubscriber(configuration, netManager, projectVersionProvider, Logger);
        var packageListener = new ServerListener(listener,
            networkReceiveEventSubscriber,
            netManager,
            connectedPeers,
            endpointsStorage,
            Logger,
            configuration,
            DefaultPackageSchema,
            connectionRequestEventSubscriber);

        return new FatNetLib(client, endpointRecorder, packageListener);
    }
}