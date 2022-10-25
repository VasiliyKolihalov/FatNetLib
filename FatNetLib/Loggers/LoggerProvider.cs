using Microsoft.Extensions.Logging;

namespace Kolyhalov.FatNetLib.Loggers;

public class LoggerProvider : ILoggerProvider
{
    public ILogger? Logger { get; set; }
}