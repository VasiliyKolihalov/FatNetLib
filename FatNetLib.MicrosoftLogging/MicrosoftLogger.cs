using Microsoft.Extensions.Logging;
using ILogger = Kolyhalov.FatNetLib.Core.Loggers.ILogger;

namespace Kolyhalov.FatNetLib.MicrosoftLogging;

public class MicrosoftLogger : ILogger
{
    private readonly Microsoft.Extensions.Logging.ILogger _logger;

    public MicrosoftLogger(Microsoft.Extensions.Logging.ILogger logger)
    {
        _logger = logger;
    }

    public void Debug(string message)
    {
        _logger.LogDebug(message);
    }

    public void Debug(Func<string> messageProvider)
    {
        if (!_logger.IsEnabled(LogLevel.Debug))
            return;

        _logger.LogDebug(messageProvider.Invoke());
    }

    public void Info(string message)
    {
        _logger.LogInformation(message);
    }

    public void Warn(string message)
    {
        _logger.LogWarning(message);
    }

    public void Error(Exception? exception, string message)
    {
        _logger.LogError(exception, message);
    }

    public void Error(string message)
    {
        _logger.LogError(message);
    }
}
