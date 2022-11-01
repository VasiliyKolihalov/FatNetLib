﻿using System.Collections.Generic;
using System.Linq;
using AutoFixture.NUnit3;
using FluentAssertions;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Microtypes;
using LiteNetLib;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Initializers.Controllers.Server;

public class InitializationControllerTests
{
    private IEndpointsStorage _endpointsStorage = null!;
    private InitializationController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _endpointsStorage = new EndpointsStorage();
        _controller = new InitializationController(_endpointsStorage);
    }

    [Test, AutoData]
    public void ExchangeInitEndpoints_EndpointsPackage_WriteRemoteAndReturnLocalEndpoints(int peerId)
    {
        // Arrange
        List<Endpoint> endpoints = SomeEndpoints().Where(x => x.IsInitial).ToList();
        var requestPackage = new Package
        {
            Body = new EndpointsBody { Endpoints = endpoints },
            FromPeerId = peerId
        };
        RegisterLocalEndpoints(_endpointsStorage);
        _endpointsStorage.LocalEndpoints.Add(GetExchangeInitEndpointsAsEndpoint());

        // Act
        Package responsePackage = _controller.ExchangeInitialEndpoints(requestPackage);

        // Assert
        _endpointsStorage.RemoteEndpoints[peerId].Should().BeEquivalentTo(endpoints);
        responsePackage.GetBodyAs<EndpointsBody>()!.Endpoints.Should()
            .BeEquivalentTo(endpoints);
    }

    private static LocalEndpoint GetExchangeInitEndpointsAsEndpoint()
    {
        var endpoint = new Endpoint(
            new Route("fat-net-lib/init-endpoints/exchange"),
            EndpointType.Exchanger,
            DeliveryMethod.ReliableOrdered,
            isInitial: true,
            requestSchemaPatch: new PackageSchema(),
            responseSchemaPatch: new PackageSchema());
        return new LocalEndpoint(endpoint, methodDelegate: () => { });
    }

    private static IEnumerable<Endpoint> SomeEndpoints()
    {
        return new List<Endpoint>
        {
            new(
                new Route("test-route1"),
                EndpointType.Exchanger,
                DeliveryMethod.Sequenced,
                isInitial: true,
                requestSchemaPatch: new PackageSchema(),
                responseSchemaPatch: new PackageSchema()),
            new(
                new Route("test-route2"),
                EndpointType.Receiver,
                DeliveryMethod.Unreliable,
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