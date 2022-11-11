using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Endpoints;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Middlewares;
using Kolyhalov.FatNetLib.Core.Modules;

namespace Kolyhalov.FatNetLib.Core
{
    public class FatNetLibBuilder
    {
        public Configuration? ConfigurationPatch { private get; set; } = null!;

        public PackageSchema? PackageSchemaPatch { private get; set; } = null!; // Todo: move into configuration

        public ILogger? Logger { private get; set; } = null!;

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

            var modulesContext = new ModuleContext(_dependencyContext);
            Modules.Setup(modulesContext);

            PutLogger();
            PatchConfiguration();
            PatchPackageSchema();

            return new FatNetLib(_dependencyContext.Get<IClient>(), _dependencyContext.Get<INetEventListener>());
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

        private void PutLogger()
        {
            if (Logger is null)
                return;

            _dependencyContext.Put<ILogger>(_ => Logger);
        }

        private void PatchConfiguration()
        {
            if (ConfigurationPatch is null)
                return;

            var configuration = _dependencyContext.Get<Configuration>();
            configuration.Patch(ConfigurationPatch);
        }

        private void PatchPackageSchema()
        {
            if (PackageSchemaPatch is null)
                return;

            _dependencyContext.Get<PackageSchema>("DefaultPackageSchema").Patch(PackageSchemaPatch);
        }
    }
}
