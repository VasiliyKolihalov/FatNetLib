﻿using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Controllers.Server;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Wrappers;
using Moq;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Core.Tests.Controllers;

public class ExchangeInitializersControllerTests
{
    private readonly Mock<INetPeer> _peer = new();
    private IEndpointsStorage _endpointsStorage = null!;
    private ExchangeInitializersController _controller = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _peer.Setup(_ => _.Id).Returns(Guid.NewGuid());
    }

    [SetUp]
    public void SetUp()
    {
        _endpointsStorage = new EndpointsStorage();
        _controller = new ExchangeInitializersController(_endpointsStorage);
    }

    [Test]
    public void ExchangeInitEndpoints_EndpointsPackage_WriteRemoteAndReturnLocalEndpoints()
    {
        // Arrange
        List<Endpoint> initializers = SomeEndpoints()
            .Where(_ => _.Type is EndpointType.Initializer)
            .ToList();
        RegisterLocalEndpoints(_endpointsStorage);
        _endpointsStorage.LocalEndpoints.Add(ALocalInitializer());

        // Act
        EndpointsBody result = _controller.ExchangeInitializers(
            new EndpointsBody { Endpoints = initializers }, _peer.Object);

        // Assert
        _endpointsStorage.RemoteEndpoints[_peer.Object.Id].Should().BeEquivalentTo(initializers);
        result.Endpoints.Should()
            .BeEquivalentTo(initializers);
    }

    private static LocalEndpoint ALocalInitializer()
    {
        var initializer = new Endpoint(
            new Route("fat-net-lib/initializers/exchange"),
            EndpointType.Initializer,
            Reliability.ReliableOrdered,
            requestSchemaPatch: new PackageSchema(),
            responseSchemaPatch: new PackageSchema());
        return new LocalEndpoint(initializer, action: new Func<Package>(() => new Package()));
    }

    private static IEnumerable<Endpoint> SomeEndpoints()
    {
        return new List<Endpoint>
        {
            new Endpoint(
                new Route("test-route1"),
                EndpointType.Initializer,
                Reliability.Sequenced,
                requestSchemaPatch: new PackageSchema(),
                responseSchemaPatch: new PackageSchema()),
            new Endpoint(
                new Route("test-route2"),
                EndpointType.Consumer,
                Reliability.Unreliable,
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
