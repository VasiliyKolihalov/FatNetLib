using Kolyhalov.FatNetLib.Microtypes;
using Kolyhalov.FatNetLib.Middlewares;
using Microsoft.Extensions.Logging;

namespace Kolyhalov.FatNetLib;

public abstract class FatNetLibBuilder
{
    public Port Port { get; init; } = null!;
    public ILogger? Logger { get; init; }
    public TimeSpan? ExchangeTimeout { get; init; }
    public IList<IMiddleware> SendingMiddlewares { get; init; } = new List<IMiddleware>();
    public IList<IMiddleware> ReceivingMiddlewares { get; init; } = new List<IMiddleware>();

    public IEndpointRecorder Endpoints { get; } = new EndpointRecorder();
    public abstract FatNetLib Build();
}