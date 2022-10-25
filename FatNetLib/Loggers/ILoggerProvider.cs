using Microsoft.Extensions.Logging;

namespace Kolyhalov.FatNetLib.Loggers;

public interface ILoggerProvider
{
    public ILogger? Logger { get; set; }
}