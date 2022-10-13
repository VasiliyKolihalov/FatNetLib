using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture.NUnit3;
using FluentAssertions;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Microtypes;
using LiteNetLib;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using static Moq.Times;

namespace Kolyhalov.FatNetLib.Initializers.Controllers.Server;

public class ExchangeEndpointsControllerTests
{
    private IEndpointsStorage _endpointsStorage = null!;
    private JsonSerializer _jsonSerializer = null!;
    private Mock<IClient> _client = null!;
    private JsonConverter[] JsonConverters => _jsonSerializer.Converters.ToArray();
    private ExchangeEndpointsController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _endpointsStorage = new EndpointsStorage();
        _jsonSerializer = JsonSerializer.Create(
            new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new RouteConverter() }
            });
        _client = new Mock<IClient>();
        _controller = new ExchangeEndpointsController(_endpointsStorage, _client.Object, _jsonSerializer);
    }

    [Test, AutoData]
    public void Exchange_Package_SendLocalAndWriteRemoteEndpoints(int peerId)
    {
        // Arrange
        List<Endpoint> endpoints = GetEndpoints().Where(x => x.IsInitial == false).ToList();
        var sendingPackage = new Package
        {
            Route = new Route("fat-net-lib/endpoints/exchange"),
            Body = new Dictionary<string, object>
            {
                ["Endpoints"] = _endpointsStorage
                    .LocalEndpoints
                    .Select(_ => _.EndpointData)
                    .Where(x => x.IsInitial == false)
            },
            ToPeerId = peerId
        };
        _client.Setup(x => x.SendPackage(It.IsAny<Package>())).Returns(new Package
        {
            Body = new Dictionary<string, object>
            {
                ["Endpoints"] = JsonConvert.SerializeObject(endpoints, JsonConverters)
            },
            FromPeerId = peerId
        });
        IncludeLocalEndpoints(_endpointsStorage);
        var requestPackage = new Package
        {
            FromPeerId = peerId
        };

        //Act
        Package responsePackage = _controller.Exchange(requestPackage);

        //Assert
        responsePackage.Should().NotBeNull();
        _client.Verify(x => x.SendPackage(It.Is<Package>(package => PackageEquals(package, sendingPackage))), Once);
        _endpointsStorage.RemoteEndpoints[peerId].Should().BeEquivalentTo(endpoints);
    }

    private static bool PackageEquals(Package first, Package second)
    {
        if (!first.Route!.Equals(second.Route))
            return false;
        if (first.ToPeerId != second.ToPeerId)
            return false;

        var firstPackageEndpoints = first.Body!["Endpoints"].As<IEnumerable<Endpoint>>();
        var secondPackageEndpoints = second.Body!["Endpoints"].As<IEnumerable<Endpoint>>();
        if (!firstPackageEndpoints.SequenceEqual(secondPackageEndpoints, new EndpointComparer()))
            return false;

        return true;
    }

    private class EndpointComparer : IEqualityComparer<Endpoint>
    {
        public bool Equals(Endpoint? x, Endpoint? y)
        {
            if (!x!.Route.Equals(y!.Route))
                return false;

            if (x.EndpointType != y.EndpointType)
                return false;

            if (x.DeliveryMethod != y.DeliveryMethod)
                return false;

            if (x.IsInitial != y.IsInitial)
                return false;

            return true;
        }

        public int GetHashCode(Endpoint obj)
        {
            int hashCode = obj.Route.GetHashCode() ^
                           obj.EndpointType.GetHashCode() ^
                           obj.DeliveryMethod.GetHashCode() ^
                           obj.IsInitial.GetHashCode();
            return hashCode.GetHashCode();
        }
    }

    private static List<Endpoint> GetEndpoints()
    {
        var endpointType = It.IsAny<EndpointType>();
        var deliveryMethod = It.IsAny<DeliveryMethod>();

        return new List<Endpoint>
        {
            new(new Route("test-route1"), endpointType, deliveryMethod, isInitial: true),
            new(new Route("test-route2"), endpointType, deliveryMethod, isInitial: false)
        };
    }

    private static List<LocalEndpoint> GetLocalEndpoints()
    {
        Delegate methodDelegate = () => { };
        List<Endpoint> endpoints = GetEndpoints();
        var localEndpoints = new List<LocalEndpoint>();
        foreach (Endpoint endpoint in endpoints)
        {
            localEndpoints.Add(new LocalEndpoint(endpoint, methodDelegate));
        }

        return localEndpoints;
    }

    private static void IncludeLocalEndpoints(IEndpointsStorage endpointsStorage)
    {
        foreach (LocalEndpoint localEndpoint in GetLocalEndpoints())
        {
            endpointsStorage.LocalEndpoints.Add(localEndpoint);
        }
    }
}