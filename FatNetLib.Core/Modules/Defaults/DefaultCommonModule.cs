using System;
using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Couriers;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Middlewares;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Monitors;
using Kolyhalov.FatNetLib.Core.Providers;
using Kolyhalov.FatNetLib.Core.Recorders;
using Kolyhalov.FatNetLib.Core.Runners;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Subscribers;
using Kolyhalov.FatNetLib.Core.Timers;
using Kolyhalov.FatNetLib.Core.Wrappers;
using LiteNetLib;
using static Kolyhalov.FatNetLib.Core.Constants.RouteConstants.Routes;
using static Kolyhalov.FatNetLib.Core.Constants.RouteConstants.Routes.Events;
using INetEventListener = Kolyhalov.FatNetLib.Core.Subscribers.INetEventListener;
using NetManager = LiteNetLib.NetManager;

namespace Kolyhalov.FatNetLib.Core.Modules.Defaults
{
    public class DefaultCommonModule : IModule
    {
        public void Setup(IModuleContext moduleContext)
        {
            CreateLogger(moduleContext);
            CreateDefaultPackageSchema(moduleContext);
            CreateResponsePackageMonitorStorage(moduleContext);
            CreateConnectedPeers(moduleContext);
            CreateMiddlewareLists(moduleContext);
            CreateMiddlewaresRunners(moduleContext);
            CreateEndpointsInvoker(moduleContext);
            CreateEventBasedNetListener(moduleContext);
            CreateProtocolVersionProvider(moduleContext);
            CreateNetManager(moduleContext);
            CreateResponsePackageMonitor(moduleContext);
            CreateNetEventPollingTimer(moduleContext);
            CreateNetEventListener(moduleContext);
            CreateNetworkReceiveEventSubscriber(moduleContext);
            RegisterEvents(moduleContext);
        }

        private static void CreateLogger(IModuleContext moduleContext)
        {
            moduleContext.PutDependency<ILogger>(_ => new NullLogger());
        }

        private static void CreateDefaultPackageSchema(IModuleContext moduleContext)
        {
            moduleContext.PutDependency("DefaultPackageSchema", _ => new PackageSchema
            {
                { nameof(Package.Route), typeof(Route) },
                { nameof(Package.Body), typeof(IDictionary<string, object>) },
                { nameof(Package.ExchangeId), typeof(Guid) },
                { nameof(Package.IsResponse), typeof(bool) }
            });
        }

        private static void CreateResponsePackageMonitorStorage(IModuleContext moduleContext)
        {
            moduleContext.PutDependency<IResponsePackageMonitorStorage>(_ => new ResponsePackageMonitorStorage());
        }

        private static void CreateConnectedPeers(IModuleContext moduleContext)
        {
            moduleContext.PutDependency("ConnectedPeers", _ => new List<INetPeer>());
        }

        private static void CreateMiddlewareLists(IModuleContext moduleContext)
        {
            moduleContext
                .PutDependency("SendingMiddlewares", _ => new List<IMiddleware>())
                .PutDependency("ReceivingMiddlewares", _ => new List<IMiddleware>());
        }

        private static void CreateMiddlewaresRunners(IModuleContext moduleContext)
        {
            moduleContext.PutDependency(
                "SendingMiddlewaresRunner",
                _ => new MiddlewaresRunner(_.Get<IList<IMiddleware>>("SendingMiddlewares")));
            moduleContext.PutDependency(
                "ReceivingMiddlewaresRunner",
                _ => new MiddlewaresRunner(_.Get<IList<IMiddleware>>("ReceivingMiddlewares")));
        }

        private static void CreateEndpointsInvoker(IModuleContext moduleContext)
        {
            moduleContext.PutDependency<IEndpointsInvoker>(_ => new EndpointsInvoker());
        }

        private static void CreateEventBasedNetListener(IModuleContext moduleContext)
        {
            moduleContext.PutDependency(_ => new EventBasedNetListener());
        }

        private static void CreateProtocolVersionProvider(IModuleContext moduleContext)
        {
            moduleContext.PutDependency<IProtocolVersionProvider>(_ => new ProtocolVersionProvider());
        }

        private static void CreateNetManager(IModuleContext moduleContext)
        {
            moduleContext.PutDependency<INetManager>(_ =>
                new Wrappers.NetManager(new NetManager(_.Get<EventBasedNetListener>())));
        }

        private static void CreateResponsePackageMonitor(IModuleContext moduleContext)
        {
            moduleContext.PutDependency<IResponsePackageMonitor>(_ => new ResponsePackageMonitor(
                _.Get<Configuration>().ExchangeTimeout!.Value,
                new Monitor(),
                _.Get<IResponsePackageMonitorStorage>()));
        }

        private static void CreateNetEventPollingTimer(IModuleContext moduleContext)
        {
            moduleContext.PutDependency(
                "NetEventPollingTimer",
                _ => new SleepBasedTimer(_.Get<Configuration>().Framerate!));
            moduleContext.PutDependency(
                "NetEventPollingTimerExceptionHandler",
                _ => new LogPollingExceptionHandler(_.Get<ILogger>()));
        }

        private static void CreateNetEventListener(IModuleContext moduleContext)
        {
            moduleContext.PutDependency<INetEventListener>(_ => new NetEventListener(
                _.Get<EventBasedNetListener>(),
                _.Get<ICourier>(),
                _.Get<INetManager>(),
                _.Get<IConnectionStarter>(),
                _.Get<ITimer>("NetEventPollingTimer"),
                _.Get<ITimerExceptionHandler>("NetEventPollingTimerExceptionHandler"),
                _.Get<ILogger>()));
        }

        private static void CreateNetworkReceiveEventSubscriber(IModuleContext moduleContext)
        {
            moduleContext.PutDependency<INetworkReceiveEventSubscriber>(_ => new NetworkReceiveEventSubscriber(
                _.Get<IResponsePackageMonitor>(),
                _.Get<IMiddlewaresRunner>("ReceivingMiddlewaresRunner"),
                _.Get<PackageSchema>("DefaultPackageSchema"),
                _,
                _.Get<IEndpointsStorage>(),
                _.Get<IEndpointsInvoker>(),
                _.Get<IMiddlewaresRunner>("SendingMiddlewaresRunner"),
                _.Get<Route>("LastInitializerRoute"),
                _.Get<ICourier>()));
        }

        private static void RegisterEvents(IModuleContext moduleContext)
        {
            moduleContext.PutScript("RegisterEventEndpoints", _ =>
            {
                _.Get<IEndpointRecorder>()
                    .AddEvent(
                        NetworkReceived,
                        package =>
                        {
                            var body = package.GetBodyAs<NetworkReceiveBody>();
                            _.Get<INetworkReceiveEventSubscriber>()
                                .Handle(body.Peer, body.PacketReader, body.Reliability);
                        })
                    .AddEvent(
                        PeerConnected,
                        package =>
                        {
                            var body = package.GetBodyAs<INetPeer>();
                            _.Get<IPeerConnectedEventSubscriber>()
                                .Handle(body);
                        })
                    .AddEvent(
                        PeerDisconnected,
                        package =>
                        {
                            var body = package.GetBodyAs<PeerDisconnectedBody>();
                            _.Get<IPeerDisconnectedEventSubscriber>()
                                .Handle(body.Peer, body.DisconnectInfo);
                        })
                    .AddEvent(
                        Events.ConnectionRequest,
                        package =>
                        {
                            var body = package.GetBodyAs<IConnectionRequest>();
                            _.Get<IConnectionRequestEventSubscriber>()
                                .Handle(body);
                        });
            });
        }
    }
}
