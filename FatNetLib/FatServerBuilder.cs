using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Microtypes;
using Kolyhalov.FatNetLib.ResponsePackageMonitors;
using LiteNetLib;

namespace Kolyhalov.FatNetLib;

public class FatServerBuilder : FatNetLibBuilder
{
    public Count? MaxPeers { get; init; }

    public override FatNetLib Build()
    {
        var configuration = new ServerConfiguration(
            Port,
            connectionKey: string.Empty, //Todo: #24 protocol version control instead of connection key
            MaxPeers,
            Framerate,
            ExchangeTimeout);

        var monitor = new ResponsePackageMonitor(new Monitor(),
            configuration.ExchangeTimeout,
            new ResponsePackageMonitorStorage());
        
        var packageListener = new ServerListener(Listener,
            GetNetworkReceiveEventHandler(monitor),
            new NetManager(Listener),
            ConnectedPeers,
            EndpointsStorage,
            Logger,
            configuration);

        return new FatNetLib(GetClient(monitor), EndpointRecorder, packageListener);
    }
}