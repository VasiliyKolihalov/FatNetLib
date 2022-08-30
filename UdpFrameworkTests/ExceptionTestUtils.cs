using System;
using Microsoft.Extensions.Logging;
using Moq;
using static Moq.Times;

namespace UdpFrameworkTests;

public static class ExceptionTestUtils
{
    public static void VerifyLogErrorWasCalled(Mock<ILogger> logger, string message, Func<Times>? times = null)
    {
        logger.Verify(logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.Is<EventId>(eventId => eventId.Id == 0),
                It.Is<It.IsAnyType>((@object, @type) => 
                    @object.ToString() == message && @type.Name == "FormattedLogValues"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            times ?? Once);
    }
}