using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Controllers.Client;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Wrappers;
using Moq;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Core.Tests.Controllers.Client;

public class ExchangeEndpointsControllerTests
{
    private readonly Mock<INetPeer> _peer = new();
    private IEndpointsStorage _endpointsStorage = null!;
    private ExchangeEndpointsController _controller = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _peer.Setup(_ => _.Id).Returns(0);
    }

    [SetUp]
    public void SetUp()
    {
        _endpointsStorage = new EndpointsStorage();
        _controller = new ExchangeEndpointsController(_endpointsStorage);
    }

    [Test]
    public void Exchange_EndpointsPackage_WriteRemoteAndReturnLocalEndpoints()
    {
        // Arrange
        List<Endpoint> endpoints = SomeEndpoints();
        RegisterLocalEndpoints(_endpointsStorage);

        // Act
        EndpointsBody result = _controller.ExchangeEndpoints(
            new EndpointsBody { Endpoints = endpoints }, _peer.Object);

        // Assert
        _endpointsStorage.RemoteEndpoints[0].Should().BeEquivalentTo(endpoints);
        result.Endpoints.Should()
            .BeEquivalentTo(_endpointsStorage.LocalEndpoints
                .Select(_ => _.Details)
                .Where(_ => _.Type is EndpointType.Consumer or EndpointType.Exchanger));
    }

    private static List<Endpoint> SomeEndpoints()
    {
        var reliability = It.IsAny<Reliability>();

        return new List<Endpoint>
        {
            new Endpoint(
                new Route("test-route1"),
                EndpointType.Initializer,
                reliability,
                requestSchemaPatch: new PackageSchema(),
                responseSchemaPatch: new PackageSchema()),
            new Endpoint(
                new Route("test-route2"),
                EndpointType.Consumer,
                reliability,
                requestSchemaPatch: new PackageSchema(),
                responseSchemaPatch: new PackageSchema())
        };
    }

    private static List<LocalEndpoint> SomeLocalEndpoints()
    {
        return SomeEndpoints()
            .Select(endpoint => new LocalEndpoint(endpoint, action: new Func<Package>(() => new Package())))
            .ToList();
    }

    private static void RegisterLocalEndpoints(IEndpointsStorage endpointsStorage)
    {
        foreach (LocalEndpoint localEndpoint in SomeLocalEndpoints())
        {
            endpointsStorage.LocalEndpoints.Add(localEndpoint);
        }
    }
}
