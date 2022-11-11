using System;
using Kolyhalov.FatNetLib.Loggers;

namespace Kolyhalov.FatNetLib.Timer
{
    public class LogPollingExceptionHandler : ITimerExceptionHandler
    {
        private const string ThrottlingMessage = "Throttling detected while polling network events. " +
                                                 "Expected period {ExpectedPeriod}, actual period {ActualPeriod}";

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
                    ThrottlingMessage,
                    throttlingException.ExpectedPeriod,
                    throttlingException.ActualPeriod);
                return;
            }

            _logger.Error(exception, "Network events polling failed");
        }
    }
}
