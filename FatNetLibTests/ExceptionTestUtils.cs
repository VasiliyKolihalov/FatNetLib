using System;
using Microsoft.Extensions.Logging;
using Moq;
using static Moq.Times;

namespace FatNetLibTests;

public static class ExceptionTestUtils
{
    public static void VerifyLogErrorWasCalled(Mock<ILogger> logger, string message, Func<Times>? times = null)
    {
        logger.Verify(l => l.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                It.Is<EventId>(eventId => eventId.Id == 0),
                It.Is<It.IsAnyType>((@object, type) => 
                    @object.ToString() == message && type.Name == "FormattedLogValues"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            times ?? Once);
    }
}