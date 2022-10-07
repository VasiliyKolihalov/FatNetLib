using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Microtypes;
using LiteNetLib;
using Moq;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Middlewares;

public class PutLocalEndpointsMiddlewareTests
{
    private readonly PutLocalEndpointsMiddleware _middleware = new();
    private Mock<IDependencyContext> _context = null!;
    private Mock<IEndpointsStorage> _endpointsStorage = null!;

    [SetUp]
    public void SetUp()
    {
        _endpointsStorage = new Mock<IEndpointsStorage>();
        _endpointsStorage.Setup(_ => _.LocalEndpoints)
            .Returns(ALocalEndpointList());
        _context = new Mock<IDependencyContext>();
        _context.Setup(_ => _.Get<IEndpointsStorage>())
            .Returns(_endpointsStorage.Object);
    }

    [Test]
    public void Process_SomePackage_SetEndpointRoutesToBody()
    {
        // Arrange
        var package = new Package
        {
            Route = new Route("fat-net-lib/endpoints/exchange"),
            Context = _context.Object
        };

        // Act
        _middleware.Process(package);

        // Assert
        package.Body!["Endpoints"].Should().BeEquivalentTo(ALocalEndpointDataList());
    }

    [Test]
    public void Process_PackageOfAnotherRoute_SkipProcessing()
    {
        // Arrange
        var package = new Package { Route = new Route("some/endpoint") };

        // Act
        _middleware.Process(package);

        // Assert
        package.Body.Should().BeNull();
    }

    private static IList<LocalEndpoint> ALocalEndpointList()
    {
        return new List<LocalEndpoint>
        {
            new(
                new Endpoint(new Route("some/route"), EndpointType.Exchanger, DeliveryMethod.Sequenced),
                new Mock<ReceiverDelegate>().Object)
        };
    }

    private static IList<Endpoint> ALocalEndpointDataList()
    {
        return ALocalEndpointList().Select(_ => _.EndpointData).ToList();
    }
}