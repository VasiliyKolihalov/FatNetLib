using System.Collections.Generic;
using System.Linq;
using AutoFixture.NUnit3;
using FluentAssertions;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Microtypes;
using Moq;
using NUnit.Framework;
using static Moq.Times;

namespace Kolyhalov.FatNetLib.Initializers.Controllers.Server;

public class ExchangeEndpointsControllerTests
{
    private IEndpointsStorage _endpointsStorage = null!;
    private Mock<IClient> _client = null!;
    private ExchangeEndpointsController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _endpointsStorage = new EndpointsStorage();
        _client = new Mock<IClient>();
        _controller = new ExchangeEndpointsController(_endpointsStorage);
    }

    [Test, AutoData]
    public void Exchange_Package_SendLocalAndWriteRemoteEndpoints(int peerId)
    {
        // Arrange
        List<Endpoint> endpoints = SomeEndpoints().Where(x => x.IsInitial == false).ToList();
        var sendingPackage = new Package
        {
            Route = new Route("fat-net-lib/endpoints/exchange"),
            Body = new EndpointsBody
            {
                Endpoints = endpoints
            },
            ToPeerId = peerId
        };
        _client.Setup(x => x.SendPackage(It.IsAny<Package>())).Returns(new Package
        {
            Body = new EndpointsBody { Endpoints = endpoints },
            FromPeerId = peerId
        });
        var requestPackage = new Package
        {
            FromPeerId = peerId
        };
        PutClientIntoPackageContext(_client.Object, requestPackage);
        RegisterLocalEndpoints(_endpointsStorage);

        // Act
        Package responsePackage = _controller.ExchangeEndpoints(requestPackage);

        // Assert
        responsePackage.Should().NotBeNull();
        _client.Verify(x => x.SendPackage(It.Is<Package>(package => PackageEquals(package, sendingPackage))), Once);
        _endpointsStorage.RemoteEndpoints[peerId].Should().BeEquivalentTo(endpoints);
    }

    private static void PutClientIntoPackageContext(IClient client, Package package)
    {
        var context = new DependencyContext();
        context.Put(_ => client);

        package.Context = context;
    }

    private static bool PackageEquals(Package first, Package second)
    {
        if (!first.Route!.Equals(second.Route))
            return false;
        if (first.ToPeerId != second.ToPeerId)
            return false;

        var firstPackageEndpoints = first.GetBodyAs<EndpointsBody>().Endpoints.As<IEnumerable<Endpoint>>();
        var secondPackageEndpoints = second.GetBodyAs<EndpointsBody>().Endpoints.As<IEnumerable<Endpoint>>();
        return firstPackageEndpoints.SequenceEqual(secondPackageEndpoints, new EndpointComparer());
    }

    private class EndpointComparer : IEqualityComparer<Endpoint>
    {
        public bool Equals(Endpoint? x, Endpoint? y)
        {
            if (!x!.Route.Equals(y!.Route))
                return false;

            if (x.EndpointType != y.EndpointType)
                return false;

            if (x.Reliability != y.Reliability)
                return false;

            return x.IsInitial == y.IsInitial;
        }

        public int GetHashCode(Endpoint obj)
        {
            int hashCode = obj.Route.GetHashCode() ^
                           obj.EndpointType.GetHashCode() ^
                           obj.Reliability.GetHashCode() ^
                           obj.IsInitial.GetHashCode();
            return hashCode.GetHashCode();
        }
    }

    private static IEnumerable<Endpoint> SomeEndpoints()
    {
        var endpointType = It.IsAny<EndpointType>();
        var reliability = It.IsAny<Reliability>();

        return new List<Endpoint>
        {
            new(
                new Route("test-route1"),
                endpointType,
                reliability,
                isInitial: true,
                requestSchemaPatch: new PackageSchema(),
                responseSchemaPatch: new PackageSchema()),
            new(
                new Route("test-route2"),
                endpointType,
                reliability,
                isInitial: false,
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
