using System;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Loggers;
using Moq;
using NUnit.Framework;
using static Kolyhalov.FatNetLib.Core.Utils.ExceptionUtils;

namespace Kolyhalov.FatNetLib.Core.Tests.Utils;

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

        view.InnerExceptionView!.Message.Should().Be("inner-exception");
        view.InnerExceptionView!.Type.Should().Be(typeof(ArithmeticException));
        view.InnerExceptionView!.InnerExceptionView.Should().BeNull();
    }
}
