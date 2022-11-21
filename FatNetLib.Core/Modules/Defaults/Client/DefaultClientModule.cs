using System;
using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Controllers.Client;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Modules.Steps;
using Kolyhalov.FatNetLib.Core.Monitors;
using Kolyhalov.FatNetLib.Core.Providers;
using Kolyhalov.FatNetLib.Core.Recorders;
using Kolyhalov.FatNetLib.Core.Runners;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Subscribers;
using Kolyhalov.FatNetLib.Core.Subscribers.Client;
using Kolyhalov.FatNetLib.Core.Utils;
using Kolyhalov.FatNetLib.Core.Wrappers;

namespace Kolyhalov.FatNetLib.Core.Modules.Defaults.Client
{
    public class DefaultClientModule : IModule
    {
        public void Setup(IModuleContext moduleContext)
        {
            CreateConfiguration(moduleContext);
            moduleContext
                .PutModule(new DefaultCommonModule())
                .PutModule(new ClientEncryptionModule());
            CreateConnectionStarter(moduleContext);
            CreateCourier(moduleContext);
            CreateInitializersRunner(moduleContext);
            CreateSubscribers(moduleContext);
            CreateInitialEndpoints(moduleContext);
        }

        private static void CreateCourier(IModuleContext moduleContext)
        {
            moduleContext.PutDependency<ICourier>(_ => new Courier(
                _.Get<IList<INetPeer>>("ConnectedPeers"),
                _.Get<IEndpointsStorage>(),
                _.Get<IResponsePackageMonitor>(),
                _.Get<IMiddlewaresRunner>("SendingMiddlewaresRunner"),
                _.Get<IEndpointsInvoker>(),
                _.Get<ILogger>()))
                .TakeLastStep()
                .AndMoveBeforeStep(new StepId(
                    parentModuleType: typeof(DefaultCommonModule),
                    stepType: typeof(PutDependencyStep),
                    inModuleId: typeof(INetEventListener).ToDependencyId()));
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

        private static void CreateInitializersRunner(IModuleContext moduleContext)
        {
            moduleContext.PutDependency<IInitialEndpointsRunner>(_ => new InitialEndpointsRunner(
                _.Get<ICourier>(),
                _.Get<IEndpointsStorage>(),
                _));
        }

        private static void CreateSubscribers(IModuleContext moduleContext)
        {
            moduleContext.PutDependency<IPeerConnectedEventSubscriber>(_ => new ClientPeerConnectedEventSubscriber(
                _.Get<IList<INetPeer>>("ConnectedPeers"),
                _.Get<IInitialEndpointsRunner>(),
                _.Get<ILogger>()));

            moduleContext.PutDependency<IConnectionRequestEventSubscriber>(_ =>
                new ClientConnectionRequestEventSubscriber());

            moduleContext.PutDependency<IPeerDisconnectedEventSubscriber>(_ =>
                new ClientPeerDisconnectedEventSubscriber(
                    _.Get<IList<INetPeer>>("ConnectedPeers")));
        }

        private static void CreateConnectionStarter(IModuleContext moduleContext)
        {
            moduleContext
                .PutDependency<IConnectionStarter>(_ => new ClientConnectionStarter(
                    _.Get<INetManager>(),
                    _.Get<ClientConfiguration>().Address!,
                    _.Get<ClientConfiguration>().Port!,
                    _.Get<IProtocolVersionProvider>()))
                .TakeLastStep()
                .AndMoveAfterStep(new StepId(
                    parentModuleType: typeof(DefaultCommonModule),
                    stepType: typeof(PutDependencyStep),
                    inModuleId: typeof(INetManager).ToDependencyId()));
        }

        private static void CreateInitialEndpoints(IModuleContext moduleContext)
        {
            moduleContext.PutScript("CreateInitialEndpoints", _ =>
            {
                var endpointsStorage = _.Get<IEndpointsStorage>();
                var controller = new ExchangeEndpointsController(endpointsStorage);
                _.Get<IEndpointRecorder>()
                    .AddController(controller);
            });
        }
    }
}
