using System.Collections.Generic;
using System.Linq;
using AutoFixture.NUnit3;
using FluentAssertions;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Microtypes;
using LiteNetLib;
using Moq;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Initializers.Controllers.Client;

public class ExchangeEndpointsControllerTests
{
    private IEndpointsStorage _endpointsStorage = null!;
    private ExchangeEndpointsController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _endpointsStorage = new EndpointsStorage();
        _controller = new ExchangeEndpointsController(_endpointsStorage);
    }

    [Test, AutoData]
    public void Exchange_EndpointsPackage_WriteRemoteAndReturnLocalEndpoints(int peerId)
    {
        // Arrange
        List<Endpoint> endpoints = SomeEndpoints();
        var requestPackage = new Package
        {
            Body = new EndpointsBody { Endpoints = endpoints },
            FromPeerId = peerId
        };
        RegisterLocalEndpoints(_endpointsStorage);

        // Act
        Package responsePackage = _controller.ExchangeEndpoints(requestPackage);

        // Assert
        _endpointsStorage.RemoteEndpoints[peerId].Should().BeEquivalentTo(endpoints);
        responsePackage.GetBodyAs<EndpointsBody>()!.Endpoints.Should()
            .BeEquivalentTo(_endpointsStorage.LocalEndpoints
                .Select(_ => _.EndpointData)
                .Where(_ => _.IsInitial == false));
    }

    private static List<Endpoint> SomeEndpoints()
    {
        var endpointType = It.IsAny<EndpointType>();
        var deliveryMethod = It.IsAny<DeliveryMethod>();

        return new List<Endpoint>
        {
            new(
                new Route("test-route1"),
                endpointType,
                deliveryMethod,
                isInitial: false,
                requestSchemaPatch: new PackageSchema(),
                responseSchemaPatch: new PackageSchema()),
            new(
                new Route("test-route2"),
                endpointType,
                deliveryMethod,
                isInitial: true,
                requestSchemaPatch: new PackageSchema(),
                responseSchemaPatch: new PackageSchema())
        };
    }

    private static List<LocalEndpoint> SomeLocalEndpoints()
    {
        return SomeEndpoints()
            .Select(endpoint => new LocalEndpoint(endpoint, methodDelegate: () => { }))
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