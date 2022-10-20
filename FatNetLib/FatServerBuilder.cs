﻿using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Initializers.Controllers.Server;
using Kolyhalov.FatNetLib.Microtypes;
using Kolyhalov.FatNetLib.Middlewares;
using Kolyhalov.FatNetLib.Monitors;
using Kolyhalov.FatNetLib.Subscribers;
using Kolyhalov.FatNetLib.Wrappers;

namespace Kolyhalov.FatNetLib;

public class FatServerBuilder : FatNetLibBuilder
{
    public Count? MaxPeers { get; init; }

    public override FatNetLib Build()
    {
        CreateCommonDependencies();
        CreateConfiguration();
        CreateResponsePackageMonitor();
        CreateClient();
        CreateSubscribers();
        CreateConnectionStarter();
        CreateNetEventListener();
        RegisterInitialEndpoints();
        return CreateFatNetLib();
    }

    private void RegisterInitialEndpoints()
    {
        var endpointsStorage = Context.Get<IEndpointsStorage>();
        var exchangeEndpointsController = new ExchangeEndpointsController(endpointsStorage, Context.Get<IClient>());
        var initializationController = new InitializationController(endpointsStorage);

        var endpointRecorder = Context.Get<IEndpointRecorder>();
        endpointRecorder.AddController(exchangeEndpointsController);
        endpointRecorder.AddController(initializationController);
    }


    private void CreateConfiguration()
    {
        Context.Put(new ServerConfiguration(Port, MaxPeers, Framerate, ExchangeTimeout));
        Context.CopyReference(typeof(ServerConfiguration), typeof(Configuration));
    }

    private void CreateSubscribers()
    {
        Context.Put<INetworkReceiveEventSubscriber>(new NetworkReceiveEventSubscriber(
            Context.Get<IResponsePackageMonitor>(),
            Context.Get<IMiddlewaresRunner>("ReceivingMiddlewaresRunner"),
            Context.Get<PackageSchema>("DefaultPackageSchema"),
            Context,
            Context.Get<IEndpointsStorage>(),
            Context.Get<IEndpointsInvoker>(),
            Context.Get<IMiddlewaresRunner>("SendingMiddlewaresRunner"),
            Context.Get<IList<INetPeer>>("ConnectedPeers")));

        Context.Put<IPeerConnectedEventSubscriber>(new ServerPeerConnectedEventSubscriber(
            Context.Get<IList<INetPeer>>("ConnectedPeers")));

        Context.Put<IConnectionRequestEventSubscriber>(new ServerConnectionRequestEventSubscriber(
            Context.Get<ServerConfiguration>(),
            Context.Get<INetManager>(),
            Context.Get<IProtocolVersionProvider>(),
            Logger));

        Context.Put<IPeerDisconnectedEventSubscriber>(new ServerPeerDisconnectedEventSubscriber(
            Context.Get<IList<INetPeer>>("ConnectedPeers"),
            Context.Get<IEndpointsStorage>()));
    }

    private void CreateConnectionStarter()
    {
        Context.Put<IConnectionStarter>(new ServerConnectionStarter(
            Context.Get<INetManager>(),
            Context.Get<Configuration>()));
    }
}