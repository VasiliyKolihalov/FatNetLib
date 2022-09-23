using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Microtypes;
using Kolyhalov.FatNetLib.Middlewares;
using Kolyhalov.FatNetLib.NetPeers;
using Kolyhalov.FatNetLib.ResponsePackageMonitors;
using LiteNetLib;
using Microsoft.Extensions.Logging;

namespace Kolyhalov.FatNetLib;

public abstract class FatNetLibBuilder
{
    protected FatNetLibBuilder()
    {
        _receivingMiddlewaresRunner = new MiddlewaresRunner(ReceivingMiddlewares);
        _sendingMiddlewaresRunner = new MiddlewaresRunner(SendingMiddlewares);

        EndpointRecorder = new EndpointRecorder(EndpointsStorage);

        _packageHandler = new PackageHandler(EndpointsStorage,
            _endpointsInvoker,
            _receivingMiddlewaresRunner,
            _sendingMiddlewaresRunner,
            ConnectedPeers);
    }

    public Port Port { get; init; } = null!;
    public Frequency? Framerate { get; init; }
    public ILogger? Logger { get; init; }
    public TimeSpan? ExchangeTimeout { get; init; }
    public IList<IMiddleware> SendingMiddlewares { get; init; } = new List<IMiddleware>();
    public IList<IMiddleware> ReceivingMiddlewares { get; init; } = new List<IMiddleware>();

    protected readonly IEndpointsStorage EndpointsStorage = new EndpointsStorage();
    protected readonly IList<INetPeer> ConnectedPeers = new List<INetPeer>();
    protected readonly EventBasedNetListener Listener = new();
    protected readonly EndpointRecorder EndpointRecorder;

    private readonly IEndpointsInvoker _endpointsInvoker = new EndpointsInvoker();
    private readonly MiddlewaresRunner _sendingMiddlewaresRunner;
    private readonly MiddlewaresRunner _receivingMiddlewaresRunner;
    private readonly IPackageHandler _packageHandler;

    protected IClient CreateClient(IResponsePackageMonitor monitor)
    {
        return new Client(ConnectedPeers,
            EndpointsStorage,
            monitor,
            _sendingMiddlewaresRunner,
            _receivingMiddlewaresRunner);
    }

    protected INetworkReceiveEventHandler CreateNetworkReceiveEventHandler(IResponsePackageMonitor monitor)
    {
        return new NetworkReceiveEventHandler(_packageHandler, monitor);
    }

    public abstract FatNetLib Build();
}