using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Middlewares;

namespace Kolyhalov.FatNetLib.Modules;

public class ModuleContext
{
    public IEndpointRecorder EndpointRecorder { get; }
    public IEndpointsStorage EndpointsStorage { get; }
    public IList<IMiddleware> SendingMiddlewares { get; }
    public IList<IMiddleware> ReceivingMiddlewares { get; }
    public ConfigurationOptions ConfigurationOptions { get; }
    public IDependencyContext DependencyContext { get; }

    public ModuleContext(IEndpointRecorder endpointRecorder, IEndpointsStorage endpointsStorage, IList<IMiddleware> sendingMiddlewares, IList<IMiddleware> receivingMiddlewares, ConfigurationOptions configurationOptions, IDependencyContext dependencyContext)
    {
        EndpointRecorder = endpointRecorder;
        EndpointsStorage = endpointsStorage;
        SendingMiddlewares = sendingMiddlewares;
        ReceivingMiddlewares = receivingMiddlewares;
        ConfigurationOptions = configurationOptions;
        DependencyContext = dependencyContext;
    }

    public void PatchPackageSchema(PackageSchema packageSchema)
    {
        DependencyContext.Put(packageSchema);
    }
}