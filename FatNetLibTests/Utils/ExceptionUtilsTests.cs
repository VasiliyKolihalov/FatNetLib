using System;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using static Kolyhalov.FatNetLib.Utils.ExceptionUtils;
using static Moq.Times;

namespace Kolyhalov.FatNetLib.Utils;

public class ExceptionUtilsTests
{
    private Mock<ILogger> _logger = null!;
    private Mock<Action> _try = null!;

    [SetUp]
    public void SetUp()
    {
        _logger = new Mock<ILogger>();
        _try = new Mock<Action>();
    }

    [Test]
    public void CatchExceptionsTo_CorrectTry_Execute()
    {
        // Act
        CatchExceptionsTo(_logger.Object, _try.Object);

        // Assert
        _try.Verify(a => a.Invoke(), Once);
        _logger.VerifyNoOtherCalls();
    }

    [Test]
    public void CatchExceptionsTo_ThrowingTry_CatchAndLog()
    {
        // Arrange
        _try.Setup(@try => @try.Invoke())
            .Throws(new ArithmeticException());

        // Act
        CatchExceptionsTo(_logger.Object, _try.Object);

        // Assert
        _try.Verify(a => a.Invoke(), Once);
        _logger.VerifyLogError("Exception occurred", Once);
    }
    
    [Test]
    public void CatchExceptionsTo_ThrowingTryCustomExceptionMessage_CatchAndLog()
    {
        // Arrange
        _try.Setup(@try => @try.Invoke())
            .Throws(new ArithmeticException());

        // Act
        CatchExceptionsTo(_logger.Object, _try.Object, "Sh!t happened");

        // Assert
        _try.Verify(a => a.Invoke(), Once);
        _logger.VerifyLogError("Sh!t happened", Once);
    }
}