using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Middlewares;

namespace Kolyhalov.FatNetLib.Modules;

public class ModuleContext
{
    public IList<IMiddleware> SendingMiddlewares { get; }
    public IList<IMiddleware> ReceivingMiddlewares { get; }
    public IEndpointRecorder EndpointRecorder { get; }
    public IEndpointsStorage EndpointsStorage { get; }
    public ConfigurationOptions ConfigurationOptions { get; }
    public PackageSchema DefaultPackageSchema { get; }
    public IDependencyContext DependencyContext { get; }

    public ModuleContext(IDependencyContext dependencyContext, 
        IList<IMiddleware> sendingMiddlewares,
        IList<IMiddleware> receivingMiddlewares)
    {
        SendingMiddlewares = sendingMiddlewares;
        ReceivingMiddlewares = receivingMiddlewares;
        DependencyContext = dependencyContext;

        EndpointRecorder = DependencyContext.Get<IEndpointRecorder>();
        EndpointsStorage = DependencyContext.Get<IEndpointsStorage>();
        ConfigurationOptions = DependencyContext.Get<ConfigurationOptions>("ModuleConfigurationOptions");
        DefaultPackageSchema = DependencyContext.Get<PackageSchema>("DefaultPackageSchema");
    }
}