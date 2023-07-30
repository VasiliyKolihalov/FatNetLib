using System;
using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Components;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Controllers.Client;
using Kolyhalov.FatNetLib.Core.Couriers;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Monitors;
using Kolyhalov.FatNetLib.Core.Providers;
using Kolyhalov.FatNetLib.Core.Recorders;
using Kolyhalov.FatNetLib.Core.Runners;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Subscribers;
using Kolyhalov.FatNetLib.Core.Subscribers.Client;
using Kolyhalov.FatNetLib.Core.Wrappers;
using static Kolyhalov.FatNetLib.Core.Modules.ModuleId.Pointers;
using static Kolyhalov.FatNetLib.Core.Modules.Steps.StepType;

namespace Kolyhalov.FatNetLib.Core.Modules.Defaults.Client
{
    public class DefaultClientModule : IModule
    {
        public void Setup(IModuleContext moduleContext)
        {
            CreateConfiguration(moduleContext);
            CreateConnectionStarter(moduleContext);
            CreateCourier(moduleContext);
            moduleContext
                .PutModule(new DefaultCommonModule())
                .PutModule(new ClientEncryptionModule());
            CreateInitializersRunner(moduleContext);
            CreateSubscribers(moduleContext);
            CreateInitializers(moduleContext);
        }

        private static void CreateConfiguration(IModuleContext moduleContext)
        {
            moduleContext
                .PutDependency(_ => new ClientConfiguration
                {
                    Port = new Port(2812),
                    Framerate = new Frequency(60),
                    ExchangeTimeout = TimeSpan.FromSeconds(10),
                    Address = "localhost"
                })
                .PutDependency<Configuration>(_ => _.Get<ClientConfiguration>());
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
                .PutDependency<IConnectionStarter>(_ => new ClientConnectionStarter(
                    _.Get<INetManager>(),
                    _.Get<ClientConfiguration>(),
                    _.Get<IProtocolVersionProvider>()));
        }

        private static void CreateCourier(IModuleContext moduleContext)
        {
            moduleContext
                .FindStep(
                    parent: ThisModule,
                    step: PutDependency,
                    qualifier: typeof(IClientCourier))
                .AndMoveBeforeStep(
                    parent: ThisModule / typeof(DefaultCommonModule),
                    step: PutDependency,
                    qualifier: typeof(INetEventListener))
                .PutDependency<IClientCourier>(_ => new ClientCourier(
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
                .PutDependency<ICourier>(_ => _.Get<IClientCourier>());
        }

        private static void CreateInitializersRunner(IModuleContext moduleContext)
        {
            moduleContext.PutDependency<IInitializersRunner>(_ => new InitializersRunner(
                _.Get<IClientCourier>(),
                _.Get<IEndpointsStorage>(),
                _));
        }

        private static void CreateSubscribers(IModuleContext moduleContext)
        {
            moduleContext.PutController(_ =>
                new ClientPeerConnectedEventController(
                    _.Get<IList<INetPeer>>("ConnectedPeers"),
                    _.Get<IInitializersRunner>()));

            moduleContext.PutController(_ =>
                new ClientPeerDisconnectedEventController(
                    _.Get<IList<INetPeer>>("ConnectedPeers")));
        }

        private static void CreateInitializers(IModuleContext moduleContext)
        {
            moduleContext
                .PutScript("CreateInitializers", _ =>
                {
                    var endpointsStorage = _.Get<IEndpointsStorage>();
                    var controller = new ExchangeEndpointsController(endpointsStorage);
                    _.Get<IEndpointRecorder>().AddController(controller);
                });
        }
    }
}
