using System;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Loggers;
using Moq;
using NUnit.Framework;
using static Kolyhalov.FatNetLib.Core.Utils.ExceptionUtils;
using static Moq.Times;

namespace Kolyhalov.FatNetLib.Core.Tests.Utils
{
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
            var exception = new ArithmeticException();
            _try.Setup(@try => @try.Invoke())
                .Throws(exception);

            // Act
            CatchExceptionsTo(_logger.Object, _try.Object);

            // Assert
            _try.Verify(a => a.Invoke(), Once);
            _logger.Verify(x => x.Error(exception, "Exception occurred"), Once);
        }

        [Test]
        public void CatchExceptionsTo_ThrowingTryCustomMessage_CatchAndLog()
        {
            // Arrange
            var exception = new ArithmeticException();
            _try.Setup(@try => @try.Invoke())
                .Throws(exception);

            // Act
            CatchExceptionsTo(_logger.Object, _try.Object, "Sh!t happened");

            // Assert
            _try.Verify(a => a.Invoke(), Once);
            _logger.Verify(_ => _.Error(exception, "Sh!t happened"), Once);
        }

        [Test]
        public void ToEndpointRunFailedView_FromException_ReturnView()
        {
            // Act
            var view = new FatNetLibException(
                    "outer-exception",
                    new ArithmeticException("inner-exception"))
                .ToEndpointRunFailedView();

            // Assert
            view.Message.Should().Be("outer-exception");
            view.Type.Should().Be(typeof(FatNetLibException));

            view.InnerException!.Message.Should().Be("inner-exception");
            view.InnerException!.Type.Should().Be(typeof(ArithmeticException));
            view.InnerException!.InnerException.Should().BeNull();
        }
    }
}
