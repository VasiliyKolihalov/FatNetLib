using System;
using System.Threading.Tasks;
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
    public async Task InvokeConsumerAsync_CorrectCase_InvokeAction()
    {
        // Arrange
        var consumerAction = new Mock<ConsumerAction>();
        LocalEndpoint endpoint = ALocalEndpoint(EndpointType.Consumer, consumerAction);
        var requestPackage = new Package();

        // Act
        await _endpointsInvoker.InvokeConsumerAsync(endpoint, requestPackage);

        // Assert
        consumerAction.Verify(_ => _.Invoke(requestPackage), Once);
    }

    [Test]
    public async Task InvokeExchangerAsync_CorrectCase_InvokeDelegateReturnPackage()
    {
        // Arrange
        var exchangerAction = new Mock<ExchangerAction>();
        var responsePackage = new Package();
        exchangerAction.Setup(_ => _.Invoke(It.IsAny<Package>()))
            .Returns(responsePackage);
        LocalEndpoint endpoint = ALocalEndpoint(EndpointType.Exchanger, exchangerAction);
        var requestPackage = new Package();

        // Act
        Package actualResponsePackage = await _endpointsInvoker.InvokeExchangerAsync(endpoint, requestPackage);

        // Assert
        exchangerAction.Verify(_ => _.Invoke(requestPackage), Once);
        actualResponsePackage.Should().Be(responsePackage);
    }

    [Test]
    public async Task InvokeExchangerAsync_EndpointReturnsNull_Throw()
    {
        // Arrange
        var exchangerAction = new Mock<ExchangerAction>();
        exchangerAction.Setup(_ => _.Invoke(It.IsAny<Package>()))
            .Returns((Package)null!);
        LocalEndpoint endpoint = ALocalEndpoint(EndpointType.Exchanger, exchangerAction);

        // Act
        Func<Task> act = async () =>
            await _endpointsInvoker.InvokeExchangerAsync(endpoint, requestPackage: new Package());

        // Assert
        await act.Should().ThrowAsync<FatNetLibException>()
            .WithMessage("Exchanger returned null which is not allowed. + Endpoint route: test/route");
    }

    [Test]
    public async Task InvokeExchangerAsync_ResponsePackageWithAnotherRoute_Throw()
    {
        // Arrange
        var exchangerAction = new Mock<ExchangerAction>();
        var responsePackage = new Package { Route = new Route("another/route") };
        exchangerAction.Setup(_ => _.Invoke(It.IsAny<Package>()))
            .Returns(responsePackage);
        LocalEndpoint endpoint = ALocalEndpoint(EndpointType.Exchanger, exchangerAction);
        var requestPackage = new Package { Route = default };

        // Act
        Func<Task> act = async () => await _endpointsInvoker.InvokeExchangerAsync(endpoint, requestPackage);

        // Assert
        await act.Should().ThrowAsync<FatNetLibException>()
            .WithMessage("Changing response Route to another is not allowed. Endpoint route: test/route");
    }

    [Test]
    public async Task InvokeExchangerAsync_ResponsePackageWithAnotherExchangeId_Throw()
    {
        // Arrange
        var exchangerAction = new Mock<ExchangerAction>();
        var responsePackage = new Package { ExchangeId = NewGuid() };
        exchangerAction.Setup(_ => _.Invoke(It.IsAny<Package>()))
            .Returns(responsePackage);
        LocalEndpoint endpoint = ALocalEndpoint(EndpointType.Exchanger, exchangerAction);
        var requestPackage = new Package { ExchangeId = NewGuid() };

        // Act
        Func<Task> act = async () => await _endpointsInvoker.InvokeExchangerAsync(endpoint, requestPackage);

        // Assert
        await act.Should().ThrowAsync<FatNetLibException>()
            .WithMessage("Changing response ExchangeId to another is not allowed. Endpoint route: test/route");
    }

    [Test]
    public async Task InvokeExchangerAsync_ResponsePackageWithAnotherIsResponse_Throw()
    {
        // Arrange
        var exchangerAction = new Mock<ExchangerAction>();
        var responsePackage = new Package { IsResponse = false };
        exchangerAction.Setup(_ => _.Invoke(It.IsAny<Package>()))
            .Returns(responsePackage);
        LocalEndpoint endpoint = ALocalEndpoint(EndpointType.Exchanger, exchangerAction);
        var requestPackage = new Package { ExchangeId = default };

        // Act
        Func<Task> act = async () => await _endpointsInvoker.InvokeExchangerAsync(endpoint, requestPackage);

        // Assert
        await act.Should().ThrowAsync<FatNetLibException>()
            .WithMessage("Changing response IsResponse to another is not allowed. Endpoint route: test/route");
    }

    [Test]
    public async Task InvokeExchangerAsync_AsyncEndpoint_ExtractResultFromTask()
    {
        // Arrange
        var endpointReturns = It.IsAny<int>();
        Func<Package, Task<int>> action = _ => Task.FromResult(endpointReturns);
        var endpoint = new LocalEndpoint(
            new Endpoint(
                new Route("test/route"),
                EndpointType.Exchanger,
                Reliability.Sequenced,
                requestSchemaPatch: new PackageSchema(),
                responseSchemaPatch: new PackageSchema()),
            action);
        // Act
        Package package = await _endpointsInvoker.InvokeExchangerAsync(endpoint, new Package());

        // Assert
        package.Body!.Should().Be(endpointReturns);
    }

    [Test]
    public async Task InvokeEndpointAsync_EndpointThrow_Throw()
    {
        // Arrange
        var exchangerAction = new Mock<ConsumerAction>();
        exchangerAction.Setup(_ => _.Invoke(It.IsAny<Package>()))
            .Throws(new ArithmeticException("bad calculation"));
        LocalEndpoint endpoint = ALocalEndpoint(EndpointType.Consumer, exchangerAction);
        var requestPackage = new Package();

        // Act
        await _endpointsInvoker.InvokeConsumerAsync(endpoint, requestPackage);

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
