using System;
using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Middlewares;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Monitors;
using Kolyhalov.FatNetLib.Core.Providers;
using Kolyhalov.FatNetLib.Core.Runners;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Subscribers;
using Kolyhalov.FatNetLib.Core.Timers;
using Kolyhalov.FatNetLib.Core.Wrappers;
using LiteNetLib;
using ConnectionRequest = Kolyhalov.FatNetLib.Core.Wrappers.ConnectionRequest;
using INetEventListener = Kolyhalov.FatNetLib.Core.Subscribers.INetEventListener;
using Monitor = Kolyhalov.FatNetLib.Core.Wrappers.Monitor;
using NetManager = LiteNetLib.NetManager;
using NetPeer = Kolyhalov.FatNetLib.Core.Wrappers.NetPeer;

namespace Kolyhalov.FatNetLib.Core.Modules.Defaults
{
    public class DefaultCommonModule : IModule
    {
        private IDependencyContext _dependencyContext = null!;

        public void Setup(ModuleContext moduleContext)
        {
            _dependencyContext = moduleContext.DependencyContext;
            CreateDefaultPackageSchema();
            CreateResponsePackageMonitorStorage();
            CreateConnectedPeers();
            CreateMiddlewaresRunners();
            CreateEndpointsInvoker();
            CreateEventBasedNetListener();
            CreateNetManager();
            CreateProtocolVersionProvider();
            CreateResponsePackageMonitor();
            CreateNetEventPollingTimer();
            CreateNetEventListener();
            CreateNetworkReceiveEventSubscriber();
            RegisterEventEndpoints(moduleContext);
        }

        public IList<IModule>? ChildModules => null;

        private void CreateDefaultPackageSchema()
        {
            _dependencyContext.Put("DefaultPackageSchema", _ => new PackageSchema
            {
                { nameof(Package.Route), typeof(Route) },
                { nameof(Package.Body), typeof(IDictionary<string, object>) },
                { nameof(Package.ExchangeId), typeof(Guid) },
                { nameof(Package.IsResponse), typeof(bool) }
            });
        }

        private void CreateResponsePackageMonitorStorage()
        {
            _dependencyContext.Put<IResponsePackageMonitorStorage>(_ => new ResponsePackageMonitorStorage());
        }

        private void CreateConnectedPeers()
        {
            _dependencyContext.Put("ConnectedPeers", _ => new List<INetPeer>());
        }

        private void CreateMiddlewaresRunners()
        {
            _dependencyContext.Put(
                "SendingMiddlewaresRunner",
                context => new MiddlewaresRunner(context.Get<IList<IMiddleware>>("SendingMiddlewares")));
            _dependencyContext.Put(
                "ReceivingMiddlewaresRunner",
                context => new MiddlewaresRunner(context.Get<IList<IMiddleware>>("ReceivingMiddlewares")));
        }

        private void CreateEndpointsInvoker()
        {
            _dependencyContext.Put<IEndpointsInvoker>(_ => new EndpointsInvoker());
        }

        private void CreateEventBasedNetListener()
        {
            _dependencyContext.Put(_ => new EventBasedNetListener());
        }

        private void CreateNetManager()
        {
            _dependencyContext.Put<INetManager>(context =>
                new Wrappers.NetManager(new NetManager(context.Get<EventBasedNetListener>())));
        }

        private void CreateProtocolVersionProvider()
        {
            _dependencyContext.Put<IProtocolVersionProvider>(_ => new ProtocolVersionProvider());
        }

        private void CreateResponsePackageMonitor()
        {
            _dependencyContext.Put<IResponsePackageMonitor>(context => new ResponsePackageMonitor(
                context.Get<Configuration>().ExchangeTimeout!.Value,
                new Monitor(),
                context.Get<IResponsePackageMonitorStorage>()));
        }

        private void CreateNetEventPollingTimer()
        {
            _dependencyContext.Put(
                "NetEventPollingTimer",
                context => new SleepBasedTimer(context.Get<Configuration>().Framerate!));
            _dependencyContext.Put(
                "NetEventPollingTimerExceptionHandler",
                context => new LogPollingExceptionHandler(context.Get<ILogger>()));
        }

        private void CreateNetEventListener()
        {
            _dependencyContext.Put<INetEventListener>(context => new NetEventListener(
                context.Get<EventBasedNetListener>(),
                context.Get<ICourier>(),
                context.Get<INetManager>(),
                context.Get<IConnectionStarter>(),
                context.Get<ITimer>("NetEventPollingTimer"),
                context.Get<ITimerExceptionHandler>("NetEventPollingTimerExceptionHandler"),
                context.Get<ILogger>()));
        }

        private void CreateNetworkReceiveEventSubscriber()
        {
            _dependencyContext.Put<INetworkReceiveEventSubscriber>(context => new NetworkReceiveEventSubscriber(
                context.Get<IResponsePackageMonitor>(),
                context.Get<IMiddlewaresRunner>("ReceivingMiddlewaresRunner"),
                context.Get<PackageSchema>("DefaultPackageSchema"),
                context,
                context.Get<IEndpointsStorage>(),
                context.Get<IEndpointsInvoker>(),
                context.Get<IMiddlewaresRunner>("SendingMiddlewaresRunner"),
                context.Get<IList<INetPeer>>("ConnectedPeers")));
        }

        private static void RegisterEventEndpoints(ModuleContext moduleContext)
        {
            moduleContext.EndpointRecorder
                .AddEvent(
                    new Route("fat-net-lib/events/network-receive/handle"),
                    package =>
                    {
                        var body = package.GetBodyAs<NetworkReceiveBody>();
                        moduleContext.DependencyContext.Get<INetworkReceiveEventSubscriber>()
                            .Handle(body.NetPeer, body.PacketReader, body.Reliability);
                    })
                .AddEvent(
                    new Route("fat-net-lib/events/peer-connected/handle"),
                    package =>
                    {
                        var body = package.GetBodyAs<NetPeer>();
                        moduleContext.DependencyContext.Get<IPeerConnectedEventSubscriber>()
                            .Handle(body);
                    })
                .AddEvent(
                    new Route("fat-net-lib/events/peer-disconnected/handle"),
                    package =>
                    {
                        var body = package.GetBodyAs<PeerDisconnectedBody>();
                        moduleContext.DependencyContext.Get<IPeerDisconnectedEventSubscriber>()
                            .Handle(body.NetPeer, body.DisconnectInfo);
                    })
                .AddEvent(
                    new Route("fat-net-lib/events/connection-request/handle"),
                    package =>
                    {
                        var body = package.GetBodyAs<ConnectionRequest>();
                        moduleContext.DependencyContext.Get<IConnectionRequestEventSubscriber>()
                            .Handle(body);
                    });
        }
    }
}
