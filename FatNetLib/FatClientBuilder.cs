using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Middlewares;
using Kolyhalov.FatNetLib.Monitors;
using Kolyhalov.FatNetLib.Subscribers;
using Kolyhalov.FatNetLib.Wrappers;
using LiteNetLib;
using NetManager = Kolyhalov.FatNetLib.Wrappers.NetManager;
using Monitor = Kolyhalov.FatNetLib.Wrappers.Monitor;

namespace Kolyhalov.FatNetLib;

public class FatClientBuilder : FatNetLibBuilder
{
    public string Address { get; init; } = null!;

    public override FatNetLib Build()
    {
        // todo: think about pico di frameworks to replace this mess
        var configuration = new ClientConfiguration(Address, Port, Framerate, ExchangeTimeout);
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
        var protocolVersionProvider = new ProtocolVersionProvider();
        var packageListener = new ClientListener(listener,
            networkReceiveEventSubscriber,
            netManager,
            connectedPeers,
            endpointsStorage,
            Logger,
            configuration,
            DefaultPackageSchema,
            protocolVersionProvider);

        return new FatNetLib(client, endpointRecorder, packageListener);
    }
}