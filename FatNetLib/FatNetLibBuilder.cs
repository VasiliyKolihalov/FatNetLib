using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Microtypes;
using Kolyhalov.FatNetLib.Middlewares;
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
    public Port Port { get; init; } = null!;
    public Frequency? Framerate { get; init; }
    public ILogger? Logger { get; init; }
    public TimeSpan? ExchangeTimeout { get; init; }
    public IList<IMiddleware> SendingMiddlewares { get; init; } = new List<IMiddleware>();
    public IList<IMiddleware> ReceivingMiddlewares { get; init; } = new List<IMiddleware>();

    protected readonly DependencyContext Context = new();

    protected void CreateCommonDependencies()
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
            Logger));
    }

    protected FatNetLib CreateFatNetLib()
    {
        return new FatNetLib(Context.Get<IClient>(),
            Context.Get<IEndpointRecorder>(),
            Context.Get<NetEventListener>());
    }

    public abstract FatNetLib Build();
}