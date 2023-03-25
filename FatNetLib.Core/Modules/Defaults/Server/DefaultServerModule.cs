using System;
using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Components;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Controllers.Server;
using Kolyhalov.FatNetLib.Core.Couriers;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Monitors;
using Kolyhalov.FatNetLib.Core.Providers;
using Kolyhalov.FatNetLib.Core.Recorders;
using Kolyhalov.FatNetLib.Core.Runners;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Subscribers;
using Kolyhalov.FatNetLib.Core.Subscribers.Server;
using Kolyhalov.FatNetLib.Core.Wrappers;
using static Kolyhalov.FatNetLib.Core.Modules.ModuleId.Pointers;
using static Kolyhalov.FatNetLib.Core.Modules.Steps.StepType;

namespace Kolyhalov.FatNetLib.Core.Modules.Defaults.Server
{
    public class DefaultServerModule : IModule
    {
        public void Setup(IModuleContext moduleContext)
        {
            CreateConfiguration(moduleContext);
            CreateConnectionStarter(moduleContext);
            CreateCourier(moduleContext);
            moduleContext
                .PutModule(new DefaultCommonModule())
                .PutModule(new ServerEncryptionModule());
            CreateSubscribers(moduleContext);
            CreateInitializers(moduleContext);
        }

        private static void CreateConfiguration(IModuleContext moduleContext)
        {
            moduleContext
                .PutDependency(_ => new ServerConfiguration
                {
                    Port = new Port(2812),
                    Framerate = new Frequency(60),
                    ExchangeTimeout = TimeSpan.FromSeconds(10),
                    MaxPeers = new Count(int.MaxValue)
                })
                .PutDependency<Configuration>(_ => _.Get<ServerConfiguration>());
        }

        private static void CreateConnectionStarter(IModuleContext moduleContext)
        {
            moduleContext
                .FindStep(
                    parent: ThisModule,
                    step: PutDependency,
                    qualifier: typeof(IConnectionStarter))
                .AndMoveAfterStep(
                    parent: ThisModule / typeof(DefaultCommonModule),
                    step: PutDependency,
                    qualifier: typeof(INetManager))
                .PutDependency<IConnectionStarter>(_ => new ServerConnectionStarter(
                    _.Get<INetManager>(),
                    _.Get<Configuration>()));
        }

        private static void CreateCourier(IModuleContext moduleContext)
        {
            moduleContext
                .FindStep(
                    parent: ThisModule,
                    step: PutDependency,
                    qualifier: typeof(IServerCourier))
                .AndMoveBeforeStep(
                    parent: ThisModule / typeof(DefaultCommonModule),
                    step: PutDependency,
                    qualifier: typeof(INetEventListener))
                .PutDependency<IServerCourier>(_ => new ServerCourier(
                    _.Get<IList<INetPeer>>("ConnectedPeers"),
                    _.Get<IEndpointsStorage>(),
                    _.Get<IResponsePackageMonitor>(),
                    _.Get<IMiddlewaresRunner>("SendingMiddlewaresRunner"),
                    _.Get<IEndpointsInvoker>(),
                    _.Get<ILogger>()));

            moduleContext
                .FindStep(
                    parent: ThisModule,
                    step: PutDependency,
                    qualifier: typeof(ICourier))
                .AndMoveBeforeStep(
                    parent: ThisModule / typeof(DefaultCommonModule),
                    step: PutDependency,
                    qualifier: typeof(INetEventListener))
                .PutDependency<ICourier>(_ => _.Get<IServerCourier>());
        }

        private static void CreateSubscribers(IModuleContext moduleContext)
        {
            moduleContext.PutController(_ =>
                new ServerPeerConnectedEventController(
                    _.Get<IList<INetPeer>>("ConnectedPeers")));

            moduleContext.PutController(_ =>
                new ServerPeerDisconnectedEventController(
                    _.Get<IList<INetPeer>>("ConnectedPeers"),
                    _.Get<IEndpointsStorage>()));

            moduleContext.PutController(_ =>
                new ServerConnectionRequestEventController(
                    _.Get<ServerConfiguration>(),
                    _.Get<INetManager>(),
                    _.Get<IProtocolVersionProvider>(),
                    _.Get<ILogger>()));
        }

        private static void CreateInitializers(IModuleContext moduleContext)
        {
            moduleContext
                .PutScript("CreateInitializers", _ =>
                {
                    var endpointsStorage = _.Get<IEndpointsStorage>();
                    var endpointRecorder = _.Get<IEndpointRecorder>();
                    endpointRecorder.AddController(new ExchangeEndpointsController(endpointsStorage));
                    endpointRecorder.AddController(new ExchangeInitializersController(endpointsStorage));
                    endpointRecorder.AddController(new FinishInitializerController());
                });
        }
    }
}
