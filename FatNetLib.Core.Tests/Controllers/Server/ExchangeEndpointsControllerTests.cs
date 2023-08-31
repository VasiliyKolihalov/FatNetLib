﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Controllers.Server;
using Kolyhalov.FatNetLib.Core.Couriers;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Utils;
using Kolyhalov.FatNetLib.Core.Wrappers;
using Moq;
using NUnit.Framework;
using static Moq.Times;

namespace Kolyhalov.FatNetLib.Core.Tests.Controllers.Server;

public class ExchangeEndpointsControllerTests
{
    private readonly Mock<INetPeer> _peer = new();
    private IEndpointsStorage _endpointsStorage = null!;
    private Mock<ICourier> _courier = null!;
    private ExchangeEndpointsController _controller = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _peer.Setup(_ => _.Id).Returns(Guid.NewGuid());
    }

    [SetUp]
    public void SetUp()
    {
        _endpointsStorage = new EndpointsStorage();
        _courier = new Mock<ICourier>();
        _controller = new ExchangeEndpointsController(_endpointsStorage);
    }

    [Test, AutoData]
    public async Task ExchangeAsync_Package_SendLocalAndWriteRemoteEndpoints()
    {
        // Arrange
        List<Endpoint> endpoints = SomeEndpoints()
            .Where(_ => _.Type is EndpointType.Consumer or EndpointType.Exchanger)
            .ToList();
        var sendingPackage = new Package
        {
            Route = new Route("fat-net-lib/endpoints/exchange"),
            Body = new EndpointsBody
            {
                Endpoints = endpoints
            },
            Receiver = _peer.Object
        };
        _courier.Setup(x => x.SendAsync(It.IsAny<Package>()))
            .Returns(Task.Run(() => (Package?)new Package
            {
                Body = new EndpointsBody { Endpoints = endpoints },
                Sender = _peer.Object
            }));
        RegisterLocalEndpoints(_endpointsStorage);

        // Act
        Package responsePackage = await _controller.ExchangeEndpointsAsync(_peer.Object, _courier.Object);

        // Assert
        responsePackage.Should().NotBeNull();
        _courier.Verify(
            x => x.SendAsync(It.Is<Package>(package => PackageEquals(package, sendingPackage))),
            Once);
        _endpointsStorage.RemoteEndpoints[_peer.Object.Id].Should().BeEquivalentTo(endpoints);
    }

    private static bool PackageEquals(Package first, Package second)
    {
        if (first.Route!.NotEquals(second.Route))
            return false;
        if (first.Receiver != second.Receiver)
            return false;

        var firstPackageEndpoints = first.GetBodyAs<EndpointsBody>().Endpoints.As<IEnumerable<Endpoint>>();
        var secondPackageEndpoints = second.GetBodyAs<EndpointsBody>().Endpoints.As<IEnumerable<Endpoint>>();
        return firstPackageEndpoints.SequenceEqual(secondPackageEndpoints, new EndpointComparer());
    }

    private class EndpointComparer : IEqualityComparer<Endpoint>
    {
        public bool Equals(Endpoint? x, Endpoint? y)
        {
            if (x!.Route.NotEquals(y!.Route))
                return false;

            if (x.Type != y.Type)
                return false;

            if (x.Reliability != y.Reliability)
                return false;

            return true;
        }

        public int GetHashCode(Endpoint obj)
        {
            int hashCode = obj.Route.GetHashCode() ^
                           obj.Type.GetHashCode() ^
                           obj.Reliability.GetHashCode();
            return hashCode.GetHashCode();
        }
    }

    private static IEnumerable<Endpoint> SomeEndpoints()
    {
        return new List<Endpoint>
        {
            new Endpoint(
                new Route("test-route1"),
                EndpointType.Initializer,
                It.IsAny<Reliability>(),
                requestSchemaPatch: new PackageSchema(),
                responseSchemaPatch: new PackageSchema()),
            new Endpoint(
                new Route("test-route2"),
                EndpointType.Consumer,
                It.IsAny<Reliability>(),
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
