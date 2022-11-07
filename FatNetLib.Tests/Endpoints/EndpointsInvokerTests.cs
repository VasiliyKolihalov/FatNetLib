using System;
using System.Reflection;
using AutoFixture.NUnit3;
using FluentAssertions;
using Kolyhalov.FatNetLib.Microtypes;
using LiteNetLib;
using Moq;
using NUnit.Framework;
using static Moq.Times;

namespace Kolyhalov.FatNetLib.Endpoints;

public class EndpointsInvokerTests
{
    private readonly EndpointsInvoker _endpointsInvoker = new();

    [Test, AutoData]
    public void InvokeReceiver_CorrectCase_InvokeDelegate(Reliability reliability)
    {
        // Arrange
        var @delegate = new Mock<ReceiverDelegate>();
        LocalEndpoint endpoint = ALocalEndpoint(EndpointType.Receiver, @delegate);
        var requestPackage = new Package();

        // Act
        _endpointsInvoker.InvokeReceiver(endpoint, requestPackage);

        // Assert
        @delegate.Verify(_ => _.Invoke(requestPackage), Once);
    }

    [Test, AutoData]
    public void InvokeExchanger_CorrectCase_InvokeDelegateReturnPackage(Reliability reliability)
    {
        // Arrange
        var @delegate = new Mock<ExchangerDelegate>();
        var responsePackage = new Package();
        @delegate.Setup(_ => _.Invoke(It.IsAny<Package>()))
            .Returns(responsePackage);
        LocalEndpoint endpoint = ALocalEndpoint(EndpointType.Exchanger, @delegate);
        var requestPackage = new Package();

        // Act
        Package actualResponsePackage = _endpointsInvoker.InvokeExchanger(endpoint, requestPackage);

        // Assert
        @delegate.Verify(_ => _.Invoke(requestPackage), Once);
        actualResponsePackage.Should().Be(responsePackage);
    }

    [Test]
    public void InvokeExchanger_EndpointReturnsNull_Throw()
    {
        // Arrange
        var @delegate = new Mock<ExchangerDelegate>();
        @delegate.Setup(_ => _.Invoke(It.IsAny<Package>()))
            .Returns((Package)null!);
        LocalEndpoint endpoint = ALocalEndpoint(EndpointType.Exchanger, @delegate);

        // Act
        Action act = () => _endpointsInvoker.InvokeExchanger(endpoint, requestPackage: new Package());

        // Assert
        act.Should().Throw<FatNetLibException>()
            .WithMessage("Exchanger cannot return null");
    }

    [Test]
    public void InvokeExchanger_ResponsePackageWithAnotherRoute_Throw()
    {
        // Arrange
        var @delegate = new Mock<ExchangerDelegate>();
        var responsePackage = new Package { Route = new Route("another/route") };
        @delegate.Setup(_ => _.Invoke(It.IsAny<Package>()))
            .Returns(responsePackage);
        LocalEndpoint endpoint = ALocalEndpoint(EndpointType.Exchanger, @delegate);
        var requestPackage = new Package();

        // Act
        Action act = () => _endpointsInvoker.InvokeExchanger(endpoint, requestPackage);

        // Assert
        act.Should().Throw<FatNetLibException>()
            .WithMessage("Pointing response packages to another route is not allowed");
    }

    [Test]
    public void InvokeExchanger_ResponsePackageWithAnotherExchangeId_Throw()
    {
        // Arrange
        var @delegate = new Mock<ExchangerDelegate>();
        var responsePackage = new Package { ExchangeId = Guid.NewGuid() };
        @delegate.Setup(_ => _.Invoke(It.IsAny<Package>()))
            .Returns(responsePackage);
        LocalEndpoint endpoint = ALocalEndpoint(EndpointType.Exchanger, @delegate);
        var requestPackage = new Package();

        // Act
        Action act = () => _endpointsInvoker.InvokeExchanger(endpoint, requestPackage);

        // Assert
        act.Should().Throw<FatNetLibException>()
            .WithMessage("Changing response exchangeId to another is not allowed");
    }

    [Test]
    public void InvokeEndpoint_EndpointThrow_Throw()
    {
        // Arrange
        var @delegate = new Mock<ReceiverDelegate>();
        @delegate.Setup(_ => _.Invoke(It.IsAny<Package>()))
            .Throws(new ArithmeticException("bad calculation"));
        LocalEndpoint endpoint = ALocalEndpoint(EndpointType.Receiver, @delegate);
        var requestPackage = new Package();

        // Act
        Action act = () => _endpointsInvoker.InvokeReceiver(endpoint, requestPackage);

        // Assert
        act.Should().Throw<FatNetLibException>()
            .WithMessage("Endpoint invocation failed")
            .WithInnerException(typeof(TargetInvocationException))
            .WithInnerException(typeof(ArithmeticException))
            .WithMessage("bad calculation");
    }

    private static LocalEndpoint ALocalEndpoint(EndpointType endpointType, IMock<Delegate> @delegate)
    {
        return new LocalEndpoint(
            new Endpoint(
                new Route("test/route"),
                endpointType,
                Reliability.Sequenced,
                isInitial: false,
                requestSchemaPatch: new PackageSchema(),
                responseSchemaPatch: new PackageSchema()),
            @delegate.Object);
    }
}
