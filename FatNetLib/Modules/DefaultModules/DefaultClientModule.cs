using System;
using System.Collections.Generic;
using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Initializers;
using Kolyhalov.FatNetLib.Initializers.Controllers.Client;
using Kolyhalov.FatNetLib.Loggers;
using Kolyhalov.FatNetLib.Microtypes;
using Kolyhalov.FatNetLib.Modules.Encryption;
using Kolyhalov.FatNetLib.Subscribers;
using Kolyhalov.FatNetLib.Wrappers;

namespace Kolyhalov.FatNetLib.Modules.DefaultModules
{
    public class DefaultClientModule : IModule
    {
        private IDependencyContext _dependencyContext = null!;

        public void Setup(ModuleContext moduleContext)
        {
            _dependencyContext = moduleContext.DependencyContext;
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
                context.Get<IClient>(),
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
