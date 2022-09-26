using System;
using Microsoft.Extensions.Logging;
using Moq;
using static Moq.Times;

namespace FatNetLibTests;

public static class LogTestUtils
{
    public static void VerifyLogError(this Mock<ILogger> logger, string message, Func<Times>? times = null)
    {
        VerifyLog(logger, message, LogLevel.Error, times);
    }

    public static void VerifyLogWarning(this Mock<ILogger> logger, string message, Func<Times>? times = null)
    {
        VerifyLog(logger, message, LogLevel.Warning, times);
    }
    
    private static void VerifyLog(this Mock<ILogger> logger, string message, LogLevel logLevel, Func<Times>? times = null)
    {
        logger.Verify(l => l.Log(
                It.Is<LogLevel>(lvl => lvl == logLevel),
                It.Is<EventId>(eventId => eventId.Id == 0),
                It.Is<It.IsAnyType>((@object, type) => 
                    @object.ToString() == message && type.Name == "FormattedLogValues"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            times ?? Once);
    }
}