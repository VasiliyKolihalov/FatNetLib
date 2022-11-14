using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Middlewares;
using Kolyhalov.FatNetLib.Core.Recorders;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Subscribers;

namespace Kolyhalov.FatNetLib.Core
{
    public class FatNetLibBuilder
    {
        public Configuration? ConfigurationPatch { private get; set; } = null!;

        public IModulesRecorder Modules { get; } = new ModulesRecorder();

        public IEndpointRecorder Endpoints { get; }

        public IList<IMiddleware> SendingMiddlewares { get; } = new List<IMiddleware>();

        public IList<IMiddleware> ReceivingMiddlewares { get; } = new List<IMiddleware>();

        private readonly IDependencyContext _dependencyContext = new DependencyContext();

        public FatNetLibBuilder()
        {
            CreateEndpointsStorage();
            CreateEndpointRecorder();
            Endpoints = _dependencyContext.Get<IEndpointRecorder>();
        }

        public FatNetLib Build()
        {
            PutMiddlewares();
            PutNullLogger();

            var modulesContext = new ModuleContext(_dependencyContext);
            Modules.Setup(modulesContext);

            PatchConfiguration();

            return new FatNetLib(_dependencyContext.Get<ICourier>(), _dependencyContext.Get<INetEventListener>());
        }

        private void CreateEndpointsStorage()
        {
            _dependencyContext.Put<IEndpointsStorage>(_ => new EndpointsStorage());
        }

        private void CreateEndpointRecorder()
        {
            _dependencyContext.Put<IEndpointRecorder>(context =>
                new EndpointRecorder(context.Get<IEndpointsStorage>()));
        }

        private void PutMiddlewares()
        {
            _dependencyContext.Put("SendingMiddlewares", _ => SendingMiddlewares);
            _dependencyContext.Put("ReceivingMiddlewares", _ => ReceivingMiddlewares);
        }

        private void PutNullLogger()
        {
            _dependencyContext.Put<ILogger>(_ => new NullLogger());
        }

        private void PatchConfiguration()
        {
            if (ConfigurationPatch is null)
                return;

            var configuration = _dependencyContext.Get<Configuration>();
            configuration.Patch(ConfigurationPatch);

            if (configuration.PackageSchemaPatch != null)
                _dependencyContext.Get<PackageSchema>("DefaultPackageSchema").Patch(configuration.PackageSchemaPatch);
        }
    }
}
