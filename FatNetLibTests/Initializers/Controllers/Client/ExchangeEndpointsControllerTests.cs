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
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Kolyhalov.FatNetLib.Initializers.Controllers.Client;

public class ExchangeEndpointsControllerTests
{
    private IEndpointsStorage _endpointsStorage = null!;
    private JsonSerializer _jsonSerializer = null!;
    private ExchangeEndpointsController _controller = null!;
    private JsonConverter[] JsonConverters => _jsonSerializer.Converters.ToArray();

    [SetUp]
    public void SetUp()
    {
        _endpointsStorage = new EndpointsStorage();
        _jsonSerializer = JsonSerializer.Create(
            new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new RouteConverter() }
            });
        _controller = new ExchangeEndpointsController(_endpointsStorage, _jsonSerializer);
    }

    [Test, AutoData]
    public void Exchange_EndpointsPackage_WriteRemoteAndReturnLocalEndpoints(int peerId)
    {
        // Arrange
        List<Endpoint> endpoints = GetEndpoints();
        var requestPackage = new Package
        {
            Body = new Dictionary<string, object>
            {
                ["Endpoints"] = JsonConvert.SerializeObject(endpoints, JsonConverters)
            },
            FromPeerId = peerId
        };
        IncludeLocalEndpoints(_endpointsStorage);

        //Act
        Package responsePackage = _controller.Exchange(requestPackage);

        //Assert
        _endpointsStorage.RemoteEndpoints[peerId].Should().BeEquivalentTo(endpoints);
        responsePackage.Body!["Endpoints"].Should()
            .BeEquivalentTo(_endpointsStorage.LocalEndpoints
                .Select(x => x.EndpointData)
                .Where(x => x.IsInitial == false));
    }

    private static List<Endpoint> GetEndpoints()
    {
        var endpointType = It.IsAny<EndpointType>();
        var deliveryMethod = It.IsAny<DeliveryMethod>();

        return new List<Endpoint>
        {
            new(new Route("test-route1"), endpointType, deliveryMethod, isInitial: false),
            new(new Route("test-route2"), endpointType, deliveryMethod, isInitial: true)
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