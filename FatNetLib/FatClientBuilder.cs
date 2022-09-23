﻿using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.ResponsePackageMonitors;
using LiteNetLib;

namespace Kolyhalov.FatNetLib;

public class FatClientBuilder : FatNetLibBuilder
{
    public string Address { get; init; } = null!;

    public override FatNetLib Build()
    {
        var configuration = new ClientConfiguration(
            Address,
            Port,
            connectionKey: string.Empty, // Todo: ticket #24 protocol version control instead of connection key
            Framerate,
            ExchangeTimeout);

        var monitor = new ResponsePackageMonitor(new Monitor(),
            configuration.ExchangeTimeout,
            new ResponsePackageMonitorStorage());

        var packageListener = new ClientListener(Listener,
            CreateNetworkReceiveEventHandler(monitor),
            new NetManager(Listener),
            ConnectedPeers,
            EndpointsStorage,
            Logger,
            configuration);

        return new FatNetLib(CreateClient(monitor), EndpointRecorder, packageListener);
    }
}