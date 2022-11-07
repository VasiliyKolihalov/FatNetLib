using System.Collections.Generic;
using FluentAssertions;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Microtypes;
using Moq;
using NUnit.Framework;
using static Moq.Times;

namespace Kolyhalov.FatNetLib.Initializers;

public class InitialEndpointsRunnerTests
{
    private const int ServerPeerId = 0;
    private readonly Route _initialExchangeEndpointsRoute = new("fat-net-lib/init-endpoints/exchange");
    private InitialEndpointsRunner _runner = null!;
    private Mock<IClient> _client = null!;
    private IEndpointsStorage _endpointsStorage = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _client = new Mock<IClient>();
        _endpointsStorage = new EndpointsStorage();
        var context = new Mock<IDependencyContext>();
        _runner = new InitialEndpointsRunner(_client.Object, _endpointsStorage, context.Object);
    }

    [Test]
    public void Run_CorrectConfiguration_CallInitialEndpoints()
    {
        // Arrange
        _endpointsStorage.LocalEndpoints.Add(AnInitialLocalEndpoint("test/client/init/endpoint"));
        _client.Setup(_ => _.SendPackage(It.IsAny<Package>()))
            .Returns(new Package
            {
                Body = new EndpointsBody
                {
                    Endpoints = new List<Endpoint> { AnInitialEndpoint("test/server/init/endpoint") }
                }
            });

        // Act
        _runner.Run();

        // Assert
        _endpointsStorage.RemoteEndpoints[ServerPeerId][0].Route
            .Should().BeEquivalentTo(_initialExchangeEndpointsRoute);
        _endpointsStorage.RemoteEndpoints[ServerPeerId][1].Route
            .Should().BeEquivalentTo(new Route("test/server/init/endpoint"));
        _endpointsStorage.LocalEndpoints[0].EndpointData.Route
            .Should().BeEquivalentTo(new Route("test/client/init/endpoint"));
        _client.Verify(
            _ => _.SendPackage(It.Is<Package>(package =>
                    package.Route!.Equals(_initialExchangeEndpointsRoute))),
            Once);
        _client.Verify(
            _ => _.SendPackage(It.Is<Package>(package =>
                    package.Route!.Equals(new Route("test/server/init/endpoint")))),
            Once);
    }

    private static LocalEndpoint AnInitialLocalEndpoint(string route)
    {
        return new LocalEndpoint(
            AnInitialEndpoint(route),
            methodDelegate: () => new Package());
    }

    private static Endpoint AnInitialEndpoint(string route)
    {
        return new Endpoint(
            new Route(route),
            EndpointType.Exchanger,
            Reliability.ReliableSequenced,
            isInitial: true,
            requestSchemaPatch: new PackageSchema(),
            responseSchemaPatch: new PackageSchema());
    }
}
