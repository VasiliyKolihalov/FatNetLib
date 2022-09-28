using Kolyhalov.FatNetLib.Microtypes;
using Kolyhalov.FatNetLib.Middlewares;
using Microsoft.Extensions.Logging;

namespace Kolyhalov.FatNetLib;

public abstract class FatNetLibBuilder
{
    public Port Port { get; init; } = null!;
    public Frequency? Framerate { get; init; }
    public ILogger? Logger { get; init; }
    public TimeSpan? ExchangeTimeout { get; init; }
    public IList<IMiddleware> SendingMiddlewares { get; init; } = new List<IMiddleware>();
    public IList<IMiddleware> ReceivingMiddlewares { get; init; } = new List<IMiddleware>();
    protected readonly PackageSchema DefaultPackageSchema = new()
    {
        {nameof(Package.Route), typeof(Route)},
        {nameof(Package.Body), typeof(IDictionary<string, object>)},
        {nameof(Package.ExchangeId), typeof(Guid)},
        {nameof(Package.IsResponse), typeof(bool)}
    };

    public abstract FatNetLib Build();
}