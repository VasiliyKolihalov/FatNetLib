using System;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Components;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Delegates;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Models;
using Moq;
using NUnit.Framework;
using static System.Guid;
using static Moq.Times;

namespace Kolyhalov.FatNetLib.Core.Tests.Components;

public class EndpointsInvokerTests
{
    private static readonly Mock<ILogger> Logger = new();

    private readonly EndpointsInvoker _endpointsInvoker = new(new ControllerArgumentsExtractor(), Logger.Object);

    [Test]
    public void InvokeReceiver_CorrectCase_InvokeAction()
    {
        // Arrange
        var receiverAction = new Mock<ReceiverAction>();
        LocalEndpoint endpoint = ALocalEndpoint(EndpointType.Receiver, receiverAction);
        var requestPackage = new Package();

        // Act
        _endpointsInvoker.InvokeReceiver(endpoint, requestPackage);

        // Assert
        receiverAction.Verify(_ => _.Invoke(requestPackage), Once);
    }

    [Test]
    public void InvokeExchanger_CorrectCase_InvokeDelegateReturnPackage()
    {
        // Arrange
        var exchangerAction = new Mock<ExchangerAction>();
        var responsePackage = new Package();
        exchangerAction.Setup(_ => _.Invoke(It.IsAny<Package>()))
            .Returns(responsePackage);
        LocalEndpoint endpoint = ALocalEndpoint(EndpointType.Exchanger, exchangerAction);
        var requestPackage = new Package();

        // Act
        Package actualResponsePackage = _endpointsInvoker.InvokeExchanger(endpoint, requestPackage);

        // Assert
        exchangerAction.Verify(_ => _.Invoke(requestPackage), Once);
        actualResponsePackage.Should().Be(responsePackage);
    }

    [Test]
    public void InvokeExchanger_EndpointReturnsNull_Throw()
    {
        // Arrange
        var exchangerAction = new Mock<ExchangerAction>();
        exchangerAction.Setup(_ => _.Invoke(It.IsAny<Package>()))
            .Returns((Package)null!);
        LocalEndpoint endpoint = ALocalEndpoint(EndpointType.Exchanger, exchangerAction);

        // Act
        Action act = () => _endpointsInvoker.InvokeExchanger(endpoint, requestPackage: new Package());

        // Assert
        act.Should().Throw<FatNetLibException>()
            .WithMessage("Exchanger returned null which is not allowed. + Endpoint route: test/route");
    }

    [Test]
    public void InvokeExchanger_ResponsePackageWithAnotherRoute_Throw()
    {
        // Arrange
        var exchangerAction = new Mock<ExchangerAction>();
        var responsePackage = new Package { Route = new Route("another/route") };
        exchangerAction.Setup(_ => _.Invoke(It.IsAny<Package>()))
            .Returns(responsePackage);
        LocalEndpoint endpoint = ALocalEndpoint(EndpointType.Exchanger, exchangerAction);
        var requestPackage = new Package { Route = default };

        // Act
        Action act = () => _endpointsInvoker.InvokeExchanger(endpoint, requestPackage);

        // Assert
        act.Should().Throw<FatNetLibException>()
            .WithMessage("Changing response Route to another is not allowed. Endpoint route: test/route");
    }

    [Test]
    public void InvokeExchanger_ResponsePackageWithAnotherExchangeId_Throw()
    {
        // Arrange
        var exchangerAction = new Mock<ExchangerAction>();
        var responsePackage = new Package { ExchangeId = NewGuid() };
        exchangerAction.Setup(_ => _.Invoke(It.IsAny<Package>()))
            .Returns(responsePackage);
        LocalEndpoint endpoint = ALocalEndpoint(EndpointType.Exchanger, exchangerAction);
        var requestPackage = new Package { ExchangeId = NewGuid() };

        // Act
        Action act = () => _endpointsInvoker.InvokeExchanger(endpoint, requestPackage);

        // Assert
        act.Should().Throw<FatNetLibException>()
            .WithMessage("Changing response ExchangeId to another is not allowed. Endpoint route: test/route");
    }

    [Test]
    public void InvokeExchanger_ResponsePackageWithAnotherIsResponse_Throw()
    {
        // Arrange
        var exchangerAction = new Mock<ExchangerAction>();
        var responsePackage = new Package { IsResponse = false };
        exchangerAction.Setup(_ => _.Invoke(It.IsAny<Package>()))
            .Returns(responsePackage);
        LocalEndpoint endpoint = ALocalEndpoint(EndpointType.Exchanger, exchangerAction);
        var requestPackage = new Package { ExchangeId = default };

        // Act
        Action act = () => _endpointsInvoker.InvokeExchanger(endpoint, requestPackage);

        // Assert
        act.Should().Throw<FatNetLibException>()
            .WithMessage("Changing response IsResponse to another is not allowed. Endpoint route: test/route");
    }

    [Test]
    public void InvokeEndpoint_EndpointThrow_Throw()
    {
        // Arrange
        var exchangerAction = new Mock<ReceiverAction>();
        exchangerAction.Setup(_ => _.Invoke(It.IsAny<Package>()))
            .Throws(new ArithmeticException("bad calculation"));
        LocalEndpoint endpoint = ALocalEndpoint(EndpointType.Receiver, exchangerAction);
        var requestPackage = new Package();

        // Act
        _endpointsInvoker.InvokeReceiver(endpoint, requestPackage);

        // Assert
        Logger.Verify(_ => _.Error(
            It.IsAny<ArithmeticException>(),
            "Endpoint invocation failed. Endpoint route test/route"));
    }

    private static LocalEndpoint ALocalEndpoint(EndpointType endpointType, IMock<Delegate> action)
    {
        return new LocalEndpoint(
            new Endpoint(
                new Route("test/route"),
                endpointType,
                Reliability.Sequenced,
                requestSchemaPatch: new PackageSchema(),
                responseSchemaPatch: new PackageSchema()),
            action.Object);
    }
}
