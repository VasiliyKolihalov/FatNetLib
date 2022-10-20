using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Microtypes;
using Kolyhalov.FatNetLib.Middlewares;
using Kolyhalov.FatNetLib.Modules;
using Kolyhalov.FatNetLib.Monitors;
using Kolyhalov.FatNetLib.Subscribers;
using Kolyhalov.FatNetLib.Wrappers;
using LiteNetLib;
using Microsoft.Extensions.Logging;
using Monitor = Kolyhalov.FatNetLib.Wrappers.Monitor;
using NetManager = LiteNetLib.NetManager;

namespace Kolyhalov.FatNetLib;

public abstract class FatNetLibBuilder
{
    public List<IMiddleware> SendingMiddlewares { get; } = new();
    public List<IMiddleware> ReceivingMiddlewares { get; } = new();
    public IEndpointRecorder Endpoints { get; }
    public IModulesProvider Modules { get; }
    protected readonly ConfigurationOptions BuilderConfigurationOptions;

    protected readonly DependencyContext Context = new();

    protected FatNetLibBuilder(ConfigurationOptions configurationOptions)
    {
        BuilderConfigurationOptions = configurationOptions;
        CreateCommonDependencies();
        Endpoints = Context.Get<IEndpointRecorder>();
        Modules = Context.Get<IModulesProvider>();
    }

    private void CreateCommonDependencies()
    {
        CreateResponsePackageMonitorStorage();
        CreateConnectedPeers();
        CreateEndpointStorage();
        CreateMiddlewaresRunners();
        CreateEndpointRecorder();
        CreateEndpointsInvoker();
        CreateEventBasedNetListener();
        CreateNetManager();
        CreateProtocolVersionProvider();
        CreateDefaultPackageSchema();
        CreateModuleConfigurationOptions();
        CreateModulesProvider();
    }

    private void CreateResponsePackageMonitorStorage()
    {
        Context.Put<IResponsePackageMonitorStorage>(new ResponsePackageMonitorStorage());
    }

    private void CreateConnectedPeers()
    {
        Context.Put("ConnectedPeers", new List<INetPeer>());
    }

    private void CreateEndpointStorage()
    {
        Context.Put<IEndpointsStorage>(new EndpointsStorage());
    }

    private void CreateMiddlewaresRunners()
    {
        Context.Put("SendingMiddlewaresRunner", new MiddlewaresRunner(SendingMiddlewares));
        Context.Put("ReceivingMiddlewaresRunner", new MiddlewaresRunner(ReceivingMiddlewares));
    }

    private void CreateEndpointRecorder()
    {
        Context.Put<IEndpointRecorder>(new EndpointRecorder(Context.Get<IEndpointsStorage>()));
    }

    private void CreateEndpointsInvoker()
    {
        Context.Put<IEndpointsInvoker>(new EndpointsInvoker());
    }

    private void CreateEventBasedNetListener()
    {
        Context.Put(new EventBasedNetListener());
    }

    private void CreateNetManager()
    {
        Context.Put<INetManager>(new Wrappers.NetManager(new NetManager(Context.Get<EventBasedNetListener>())));
    }

    private void CreateProtocolVersionProvider()
    {
        Context.Put<IProtocolVersionProvider>(new ProtocolVersionProvider());
    }

    private void CreateDefaultPackageSchema()
    {
        Context.Put("DefaultPackageSchema", new PackageSchema
        {
            { nameof(Package.Route), typeof(Route) },
            { nameof(Package.Body), typeof(IDictionary<string, object>) },
            { nameof(Package.ExchangeId), typeof(Guid) },
            { nameof(Package.IsResponse), typeof(bool) }
        });
    }

    private void CreateModuleConfigurationOptions()
    {
        Context.Put("ModuleConfigurationOptions", new ConfigurationOptions());
    }

    private void CreateModulesProvider()
    {
        var modulesContext = new ModuleContext(Context,
            SendingMiddlewares,
            ReceivingMiddlewares);
        Context.Put<IModulesProvider>(new ModulesProvider(modulesContext));
    }

    protected void CreateResponsePackageMonitor()
    {
        Context.Put<IResponsePackageMonitor>(new ResponsePackageMonitor(new Monitor(),
            Context.Get<Configuration>().ExchangeTimeout,
            Context.Get<IResponsePackageMonitorStorage>()));
    }

    protected void CreateClient()
    {
        Context.Put<IClient>(new Client(Context.Get<IList<INetPeer>>("ConnectedPeers"),
            Context.Get<IEndpointsStorage>(),
            Context.Get<IResponsePackageMonitor>(),
            Context.Get<IMiddlewaresRunner>("SendingMiddlewaresRunner")));
    }

    protected void CreateNetEventListener()
    {
        Context.Put(new NetEventListener(Context.Get<EventBasedNetListener>(),
            Context.Get<INetworkReceiveEventSubscriber>(),
            Context.Get<IPeerConnectedEventSubscriber>(),
            Context.Get<IConnectionRequestEventSubscriber>(),
            Context.Get<IPeerDisconnectedEventSubscriber>(),
            Context.Get<INetManager>(),
            Context.Get<IConnectionStarter>(),
            Context.Get<Configuration>(),
            Context.Get<ILogger>()));
    }

    protected FatNetLib CreateFatNetLib()
    {
        return new FatNetLib(Context.Get<IClient>(), Context.Get<NetEventListener>());
    }

    public abstract FatNetLib Build();
}