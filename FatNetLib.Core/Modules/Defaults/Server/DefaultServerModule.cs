using System;
using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Controllers.Server;
using Kolyhalov.FatNetLib.Core.Couriers;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Modules.Steps;
using Kolyhalov.FatNetLib.Core.Monitors;
using Kolyhalov.FatNetLib.Core.Providers;
using Kolyhalov.FatNetLib.Core.Recorders;
using Kolyhalov.FatNetLib.Core.Runners;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Subscribers;
using Kolyhalov.FatNetLib.Core.Subscribers.Server;
using Kolyhalov.FatNetLib.Core.Wrappers;

namespace Kolyhalov.FatNetLib.Core.Modules.Defaults.Server
{
    public class DefaultServerModule : IModule
    {
        public void Setup(IModuleContext moduleContext)
        {
            CreateConfiguration(moduleContext);
            moduleContext
                .PutModule(new DefaultCommonModule())
                .PutModule(new ServerEncryptionModule());
            CreateConnectionStarter(moduleContext);
            CreateCourier(moduleContext);
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
                .PutDependency<IConnectionStarter>(_ => new ServerConnectionStarter(
                    _.Get<INetManager>(),
                    _.Get<Configuration>().Port!))
                .TakeLastStep()
                .AndMoveAfterStep(new StepId(
                    parentModuleType: typeof(DefaultCommonModule),
                    stepType: typeof(PutDependencyStep),
                    qualifier: typeof(INetManager)));
        }

        private static void CreateCourier(IModuleContext moduleContext)
        {
            moduleContext
                .PutDependency<IServerCourier>(_ => new ServerCourier(
                    _.Get<IList<INetPeer>>("ConnectedPeers"),
                    _.Get<IEndpointsStorage>(),
                    _.Get<IResponsePackageMonitor>(),
                    _.Get<IMiddlewaresRunner>("SendingMiddlewaresRunner"),
                    _.Get<IEndpointsInvoker>(),
                    _.Get<ILogger>()))
                .TakeLastStep()
                .AndMoveBeforeStep(
                    parent: typeof(DefaultCommonModule),
                    step: typeof(PutDependencyStep),
                    qualifier: typeof(INetEventListener));

            moduleContext
                .PutDependency<ICourier>(_ => _.Get<IServerCourier>())
                .TakeLastStep()
                .AndMoveBeforeStep(
                    parent: typeof(DefaultCommonModule),
                    step: typeof(PutDependencyStep),
                    qualifier: typeof(INetEventListener));
        }

        private static void CreateSubscribers(IModuleContext moduleContext)
        {
            moduleContext.PutDependency<IPeerConnectedEventSubscriber>(
                _ => new ServerPeerConnectedEventSubscriber(
                    _.Get<IList<INetPeer>>("ConnectedPeers")));

            moduleContext.PutDependency<IConnectionRequestEventSubscriber>(_ =>
                new ServerConnectionRequestEventSubscriber(
                    _.Get<ServerConfiguration>().MaxPeers!,
                    _.Get<INetManager>(),
                    _.Get<IProtocolVersionProvider>(),
                    _.Get<ILogger>()));

            moduleContext.PutDependency<IPeerDisconnectedEventSubscriber>(_ =>
                new ServerPeerDisconnectedEventSubscriber(
                    _.Get<IList<INetPeer>>("ConnectedPeers"),
                    _.Get<IEndpointsStorage>()));
        }

        private static void CreateInitializers(IModuleContext moduleContext)
        {
            moduleContext
                .PutScript("CreateInitializers", _ =>
                {
                    var endpointsStorage = _.Get<IEndpointsStorage>();
                    var exchangeEndpointsController = new ExchangeEndpointsController(endpointsStorage);
                    var initializationController = new ExchangeInitializersController(endpointsStorage);
                    var endpointRecorder = _.Get<IEndpointRecorder>();
                    endpointRecorder.AddController(exchangeEndpointsController);
                    endpointRecorder.AddController(initializationController);
                })
                .PutDependency("LastInitializerRoute", _ => new Route("fat-net-lib/endpoints/exchange"))
                .TakeLastStep()
                .AndMoveBeforeStep(
                    parent: typeof(DefaultCommonModule),
                    step: typeof(PutDependencyStep),
                    qualifier: typeof(INetworkReceiveEventSubscriber));
        }
    }
}
