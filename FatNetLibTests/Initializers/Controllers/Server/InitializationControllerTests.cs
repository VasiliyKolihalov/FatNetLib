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

namespace Kolyhalov.FatNetLib.Initializers.Controllers.Server;

public class InitializationControllerTests
{
    private IEndpointsStorage _endpointsStorage = null!;
    private JsonSerializer _jsonSerializer = null!;
    private JsonConverter[] JsonConverters => _jsonSerializer.Converters.ToArray();
    private InitializationController _controller = null!;


    [SetUp]
    public void SetUp()
    {
        _endpointsStorage = new EndpointsStorage();
        _jsonSerializer = JsonSerializer.Create(
            new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new RouteConverter() }
            });
        _controller = new InitializationController(_endpointsStorage, _jsonSerializer);
    }

    [Test, AutoData]
    public void ExchangeInitEndpoints_EndpointsPackage_WriteRemoteAndReturnLocalEndpoints(int peerId)
    {
        // Arrange
        List<Endpoint> endpoints = GetEndpoints().Where(x => x.IsInitial).ToList();
        var requestPackage = new Package
        {
            Body = new Dictionary<string, object>
            {
                ["Endpoints"] = JsonConvert.SerializeObject(endpoints, JsonConverters)
            },
            FromPeerId = peerId
        };
        IncludeLocalEndpoints(_endpointsStorage);
        _endpointsStorage.LocalEndpoints.Add(GetExchangeInitEndpointsAsEndpoint());

        //Act
        Package responsePackage = _controller.ExchangeInitEndpoints(requestPackage);

        //Assert
        _endpointsStorage.RemoteEndpoints[peerId].Should().BeEquivalentTo(endpoints);
        var endpointRoute = new Route("fat-net-lib/init-endpoints/exchange");
        responsePackage.Body!["Endpoints"].Should()
            .BeEquivalentTo(_endpointsStorage.LocalEndpoints
                .Select(x => x.EndpointData)
                .Where(x => x.IsInitial && !x.Route.Equals(endpointRoute))
                .Select(x => x.Route));
    }

    private LocalEndpoint GetExchangeInitEndpointsAsEndpoint()
    {
        var endpoint = new Endpoint(new Route("fat-net-lib/init-endpoints/exchange"),
            EndpointType.Exchanger,
            DeliveryMethod.ReliableOrdered,
            isInitial: true);
        return new LocalEndpoint(endpoint, methodDelegate: () => { });
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