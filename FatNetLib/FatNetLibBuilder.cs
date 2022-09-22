using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Microtypes;
using Kolyhalov.FatNetLib.Middlewares;
using Kolyhalov.FatNetLib.NetPeers;
using LiteNetLib;
using Microsoft.Extensions.Logging;

namespace Kolyhalov.FatNetLib;

public abstract class FatNetLibBuilder
{
    public Port Port { get; init; } = null!;
    public ILogger? Logger { get; init; }
    public TimeSpan? ExchangeTimeout { get; init; }
    public IList<IMiddleware> SendingMiddlewares { get; init; } = new List<IMiddleware>();
    public IList<IMiddleware> ReceivingMiddlewares { get; init; } = new List<IMiddleware>();

    protected readonly IEndpointsStorage EndpointsStorage = new EndpointsStorage();
    protected readonly IEndpointsInvoker EndpointsInvoker = new EndpointsInvoker();
    protected readonly IList<INetPeer> ConnectedPeers = new List<INetPeer>();
    protected readonly EventBasedNetListener Listener = new();

    public abstract FatNetLib Build();
}