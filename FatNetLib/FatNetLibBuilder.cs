using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Loggers;
using Kolyhalov.FatNetLib.Microtypes;
using Kolyhalov.FatNetLib.Middlewares;
using Kolyhalov.FatNetLib.Modules;
using Kolyhalov.FatNetLib.Modules.DefaultModules;
using Microsoft.Extensions.Logging;
using ILoggerProvider = Kolyhalov.FatNetLib.Loggers.ILoggerProvider;

namespace Kolyhalov.FatNetLib;

public class FatNetLibBuilder
{
    public IList<IModule> Modules { private get; init; } = null!;
    public Configuration? Configuration { private get; init; } = null!;
    public ILogger? Logger { private get; init; } = null!;
    public PackageSchema? PackageSchemaPatch { private get; init; } = null!;

    public IEndpointRecorder Endpoints { get; }
    public IList<IMiddleware> SendingMiddlewares { get; } = new List<IMiddleware>();
    public IList<IMiddleware> ReceivingMiddlewares { get; } = new List<IMiddleware>();

    private readonly IDependencyContext _dependencyContext;

    public FatNetLibBuilder()
    {
        _dependencyContext = new DependencyContext();
        CreateAndPutEndpointsStorage();
        CreateAndPutEndpointRecorder();
        Endpoints = _dependencyContext.Get<IEndpointRecorder>();
    }

    public FatNetLib Build()
    {
        if (Modules == null)
            throw new FatNetLibException("Modules is null");

        DetermineBuildTypeAndPutConfiguration();
        PutMiddlewares();
        PutLoggerProvider();
        PutDefaultPackageSchema();

        var modulesContext = new ModuleContext(_dependencyContext);
        IModulesProvider modulesProvider = new ModulesProvider(modulesContext).Register(Modules);

        if (Configuration != null)
            modulesProvider.Register(new UserConfigurationPriorityModule(userConfiguration: Configuration));
        if (PackageSchemaPatch != null)
            modulesProvider.Register(new PatchDefaultPackageSchemaModule(userPackageSchemaPatch: PackageSchemaPatch));
        if (Logger == null)
            modulesProvider.Register(new DefaultLoggerModule());

        return new FatNetLib(_dependencyContext.Get<IClient>(), _dependencyContext.Get<NetEventListener>());
    }

    private void CreateAndPutEndpointsStorage()
    {
        _dependencyContext.Put<IEndpointsStorage>(new EndpointsStorage());
    }

    private void CreateAndPutEndpointRecorder()
    {
        _dependencyContext.Put<IEndpointRecorder>(new EndpointRecorder(_dependencyContext.Get<IEndpointsStorage>()));
    }

    private void DetermineBuildTypeAndPutConfiguration()
    {
        bool isServerBuild = Modules.Any(module => module is IServerBuildTypeModule);

        if (isServerBuild == false && !Modules.Any(module => module is IClientBuildTypeModule))
            throw new FatNetLibException("Not found build type module. Unable to determine build type server/client");

        Configuration configuration = isServerBuild ? new ServerConfiguration() : new ClientConfiguration();
        _dependencyContext.Put(configuration);
        if (isServerBuild)
        {
            _dependencyContext.CopyReference(typeof(Configuration), typeof(ServerConfiguration));
            return;
        }

        _dependencyContext.CopyReference(typeof(Configuration), typeof(ClientConfiguration));
    }

    private void PutMiddlewares()
    {
        _dependencyContext.Put("SendingMiddlewares", SendingMiddlewares);
        _dependencyContext.Put("ReceivingMiddlewares", ReceivingMiddlewares);
    }

    private void PutLoggerProvider()
    {
        _dependencyContext.Put<ILoggerProvider>(new LoggerProvider());
    }

    private void PutDefaultPackageSchema()
    {
        _dependencyContext.Put("DefaultPackageSchema", new PackageSchema
        {
            { nameof(Package.Route), typeof(Route) },
            { nameof(Package.Body), typeof(IDictionary<string, object>) },
            { nameof(Package.ExchangeId), typeof(Guid) },
            { nameof(Package.IsResponse), typeof(bool) }
        });
    }
}