using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Endpoints;
using Kolyhalov.FatNetLib.Core.Middlewares;

namespace Kolyhalov.FatNetLib.Core.Modules
{
    public class ModuleContext
    {
        public IList<IMiddleware> SendingMiddlewares { get; }

        public IList<IMiddleware> ReceivingMiddlewares { get; }

        public Configuration Configuration => DependencyContext.Get<Configuration>();

        public PackageSchema DefaultPackageSchema => DependencyContext.Get<PackageSchema>("DefaultPackageSchema");

        public IEndpointRecorder EndpointRecorder { get; }

        public IEndpointsStorage EndpointsStorage { get; }

        public IDependencyContext DependencyContext { get; }

        public ModuleContext(IDependencyContext dependencyContext)
        {
            DependencyContext = dependencyContext;
            SendingMiddlewares = dependencyContext.Get<IList<IMiddleware>>("SendingMiddlewares");
            ReceivingMiddlewares = dependencyContext.Get<IList<IMiddleware>>("ReceivingMiddlewares");
            EndpointRecorder = DependencyContext.Get<IEndpointRecorder>();
            EndpointsStorage = DependencyContext.Get<IEndpointsStorage>();
        }
    }
}
