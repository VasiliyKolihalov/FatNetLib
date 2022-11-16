using System;
using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Controllers.Client;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Monitors;
using Kolyhalov.FatNetLib.Core.Providers;
using Kolyhalov.FatNetLib.Core.Runners;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Subscribers;
using Kolyhalov.FatNetLib.Core.Subscribers.Client;
using Kolyhalov.FatNetLib.Core.Wrappers;

namespace Kolyhalov.FatNetLib.Core.Modules.Defaults.Client
{
    public class DefaultClientModule : IModule
    {
        private IDependencyContext _dependencyContext = null!;

        public void Setup(ModuleContext moduleContext)
        {
            _dependencyContext = moduleContext.DependencyContext;
            CreateCourier();
            CreateConfiguration();
            CreateInitializersRunner();
            CreateSubscribers();
            CreateConnectionStarter();
            CreateInitialEndpoints(moduleContext);
        }

        public IList<IModule> ChildModules { get; } = new List<IModule>
        {
            new DefaultCommonModule(),
            new ClientEncryptionModule()
        };

        private void CreateCourier()
        {
            _dependencyContext.Put<ICourier>(context => new Courier(
                context.Get<IList<INetPeer>>("ConnectedPeers"),
                context.Get<IEndpointsStorage>(),
                context.Get<IResponsePackageMonitor>(),
                context.Get<IMiddlewaresRunner>("SendingMiddlewaresRunner")));
        }

        private void CreateConfiguration()
        {
            var defaultConfiguration = new ClientConfiguration
            {
                Port = new Port(2812),
                Framerate = new Frequency(60),
                ExchangeTimeout = TimeSpan.FromSeconds(10),
                Address = "localhost"
            };
            _dependencyContext.Put<Configuration>(_ => defaultConfiguration);
            _dependencyContext.CopyReference(typeof(Configuration), typeof(ClientConfiguration));
        }

        private void CreateInitializersRunner()
        {
            _dependencyContext.Put<IInitialEndpointsRunner>(context => new InitialEndpointsRunner(
                context.Get<ICourier>(),
                context.Get<IEndpointsStorage>(),
                context));
        }

        private void CreateSubscribers()
        {
            _dependencyContext.Put<IPeerConnectedEventSubscriber>(context => new ClientPeerConnectedEventSubscriber(
                context.Get<IList<INetPeer>>("ConnectedPeers"),
                context.Get<IInitialEndpointsRunner>(),
                context.Get<ILogger>()));

            _dependencyContext.Put<IConnectionRequestEventSubscriber>(_ =>
                new ClientConnectionRequestEventSubscriber());

            _dependencyContext.Put<IPeerDisconnectedEventSubscriber>(context =>
                new ClientPeerDisconnectedEventSubscriber(
                    context.Get<IList<INetPeer>>("ConnectedPeers")));
        }

        private void CreateConnectionStarter()
        {
            _dependencyContext.Put<IConnectionStarter>(context => new ClientConnectionStarter(
                context.Get<INetManager>(),
                context.Get<ClientConfiguration>().Address!,
                context.Get<ClientConfiguration>().Port!,
                context.Get<IProtocolVersionProvider>()));
        }

        private static void CreateInitialEndpoints(ModuleContext moduleContext)
        {
            var controller = new ExchangeEndpointsController(moduleContext.EndpointsStorage);
            moduleContext.EndpointRecorder.AddController(controller);
        }
    }
}
