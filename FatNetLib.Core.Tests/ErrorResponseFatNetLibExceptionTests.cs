using System;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Utils;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Core.Tests;

public class ErrorResponseFatNetLibExceptionTests
{
    [Test]
    public void Message_FromAnyErrorType_ReturnText()
    {
        // Act
        var exception = new ErrorResponseFatNetLibException(
            "test-message",
            new Package { Error = "test-error" });

        // Assert
        exception.Message.Should().Be("test-message. Error=test-error");
    }

    [Test]
    public void Message_FromEndpointRunFailedView_ReturnText()
    {
        // Arrange
        var response = new Package
        {
            Route = new Route("test/route"),
            Error = new FatNetLibException(
                    "outer-exception",
                    new ArithmeticException("inner-exception"))
                .ToEndpointRunFailedView()
        };

        // Act
        var exception = new ErrorResponseFatNetLibException("test-message", response);

        // Assert
        exception.Message.Should().Be(
            "test-message. Running remote endpoint failed at route test/route\n" +
            " - Type: Kolyhalov.FatNetLib.Core.Exceptions.FatNetLibException, Message: \"outer-exception\"\n" +
            " - Type: System.ArithmeticException, Message: \"inner-exception\"");
    }
}
