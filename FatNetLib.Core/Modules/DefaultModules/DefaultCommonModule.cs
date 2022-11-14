using System;
using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Endpoints;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Middlewares;
using Kolyhalov.FatNetLib.Core.Monitors;
using Kolyhalov.FatNetLib.Core.Subscribers;
using Kolyhalov.FatNetLib.Core.Timer;
using Kolyhalov.FatNetLib.Core.Wrappers;
using LiteNetLib;
using Monitor = Kolyhalov.FatNetLib.Core.Wrappers.Monitor;
using NetManager = LiteNetLib.NetManager;

namespace Kolyhalov.FatNetLib.Core.Modules.DefaultModules
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
            CreateClient();
            CreateNetEventPollingTimer();
            CreateNetEventListener();
            CreateNetworkReceiveEventSubscriber();
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

        private void CreateClient()
        {
            _dependencyContext.Put<IClient>(context => new Client(
                context.Get<IList<INetPeer>>("ConnectedPeers"),
                context.Get<IEndpointsStorage>(),
                context.Get<IResponsePackageMonitor>(),
                context.Get<IMiddlewaresRunner>("SendingMiddlewaresRunner")));
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
                context.Get<INetworkReceiveEventSubscriber>(),
                context.Get<IPeerConnectedEventSubscriber>(),
                context.Get<IConnectionRequestEventSubscriber>(),
                context.Get<IPeerDisconnectedEventSubscriber>(),
                context.Get<INetManager>(),
                context.Get<IConnectionStarter>(),
                context.Get<ITimer>("NetEventPollingTimer"),
                context.Get<ITimerExceptionHandler>("NetEventPollingTimerExceptionHandler"),
                context.Get<ILogger>()));
        }
    }
}
