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

    public void Debug(string message, params object?[] args)
    {
        _logger.LogDebug(message, args);
    }

    public void Debug(Func<string> messageProvider, params object?[] args)
    {
        if (!_logger.IsEnabled(LogLevel.Debug))
            return;

        _logger.LogDebug(messageProvider.Invoke(), args);
    }

    public void Info(string message, params object?[] args)
    {
        _logger.LogInformation(message, args);
    }

    public void Warn(string message, params object?[] args)
    {
        _logger.LogWarning(message, args);
    }

    public void Error(Exception? exception, string message, params object?[] args)
    {
        _logger.LogError(exception, message, args);
    }

    public void Error(string message, params object?[] args)
    {
        _logger.LogError(message, args);
    }
}
