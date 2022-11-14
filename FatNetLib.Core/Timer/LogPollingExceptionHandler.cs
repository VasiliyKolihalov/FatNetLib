using System;
using Kolyhalov.FatNetLib.Core.Loggers;

namespace Kolyhalov.FatNetLib.Core.Timer
{
    public class LogPollingExceptionHandler : ITimerExceptionHandler
    {
        private readonly ILogger _logger;

        public LogPollingExceptionHandler(ILogger logger)
        {
            _logger = logger;
        }

        public void Handle(Exception exception)
        {
            if (exception is ThrottlingFatNetLibException throttlingException)
            {
                _logger.Warn(
                    $@"Throttling detected while polling network events. 
                            Expected period {throttlingException.ExpectedPeriod}, 
                            actual period {throttlingException.ActualPeriod}");
                return;
            }

            _logger.Error(exception, "Network events polling failed");
        }
    }
}
