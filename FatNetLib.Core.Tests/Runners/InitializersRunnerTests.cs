using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Couriers;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Runners;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Wrappers;
using Moq;
using NUnit.Framework;
using static Kolyhalov.FatNetLib.Core.Constants.RouteConstants.Routes.Events;
using static Moq.Times;

namespace Kolyhalov.FatNetLib.Core.Tests.Runners;

public class InitializersRunnerTests
{
    private readonly Route _exchangeInitializersRoute = new("fat-net-lib/initializers/exchange");
    private InitializersRunner _runner = null!;
    private Mock<IClientCourier> _courier = null!;
    private IEndpointsStorage _endpointsStorage = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _courier = new Mock<IClientCourier>();
        _endpointsStorage = new EndpointsStorage();
        var context = new Mock<IDependencyContext>();
        _runner = new InitializersRunner(_courier.Object, _endpointsStorage, context.Object);
    }

    [Test]
    public async Task RunAsync_CorrectConfiguration_CallInitializers()
    {
        // Arrange
        _endpointsStorage.LocalEndpoints.Add(ALocalInitializer("test/client/init/endpoint"));
        _courier.Setup(_ => _.SendAsync(It.IsAny<Package>()))
            .Returns(Task.Run(() =>
            {
                var result = (Package?)new Package
                {
                    Body = new EndpointsBody
                    {
                        Endpoints = new List<Endpoint> { AnInitializer("test/server/init/endpoint") }
                    }
                };
                return result;
            }));

        var serverPeerId = Guid.NewGuid();
        var serverPeer = new Mock<INetPeer>();
        serverPeer.Setup(_ => _.Id)
            .Returns(serverPeerId);
        _courier.Setup(_ => _.ServerPeer)
            .Returns(serverPeer.Object);

        // Act
        await _runner.RunAsync();

        // Assert
        _endpointsStorage.RemoteEndpoints[serverPeerId][0].Route
            .Should().BeEquivalentTo(_exchangeInitializersRoute);
        _endpointsStorage.RemoteEndpoints[serverPeerId][1].Route
            .Should().BeEquivalentTo(new Route("test/server/init/endpoint"));
        _endpointsStorage.LocalEndpoints[0].Details.Route
            .Should().BeEquivalentTo(new Route("test/client/init/endpoint"));

        _courier.Verify(_ => _.SendAsync(It.IsAny<Package>()), times: Exactly(2));
        _courier.Verify(
            _ => _.SendAsync(It.Is<Package>(package =>
                package.Route!.Equals(_exchangeInitializersRoute))),
            Once);
        _courier.Verify(
            _ => _.SendAsync(It.Is<Package>(package =>
                package.Route!.Equals(new Route("test/server/init/endpoint")))),
            Once);

        _courier.Verify(_ => _.EmitEventAsync(It.IsAny<Package>()), Once);
        _courier.Verify(
            _ => _.EmitEventAsync(It.Is<Package>(package =>
                package.Route!.Equals(InitializationFinished))),
            Once);
    }

    private static LocalEndpoint ALocalInitializer(string route)
    {
        return new LocalEndpoint(
            AnInitializer(route),
            action: new Func<Package>(() => new Package()));
    }

    private static Endpoint AnInitializer(string route)
    {
        return new Endpoint(
            new Route(route),
            EndpointType.Initializer,
            Reliability.ReliableSequenced,
            requestSchemaPatch: new PackageSchema(),
            responseSchemaPatch: new PackageSchema());
    }
}
