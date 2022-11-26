using System;
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

namespace Kolyhalov.FatNetLib.Core.Tests.Controllers
{
    public class ExchangeInitializersControllerTests
    {
        private readonly Mock<INetPeer> _peer = new Mock<INetPeer>();
        private IEndpointsStorage _endpointsStorage = null!;
        private ExchangeInitializersController _controller = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _peer.Setup(_ => _.Id).Returns(0);
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
                .Where(_ => _.EndpointType is EndpointType.Initializer)
                .ToList();
            var requestPackage = new Package
            {
                Body = new EndpointsBody { Endpoints = initializers },
                FromPeer = _peer.Object
            };
            RegisterLocalEndpoints(_endpointsStorage);
            _endpointsStorage.LocalEndpoints.Add(ALocalInitializer());

            // Act
            Package responsePackage = _controller.ExchangeInitializers(requestPackage);

            // Assert
            _endpointsStorage.RemoteEndpoints[0].Should().BeEquivalentTo(initializers);
            responsePackage.GetBodyAs<EndpointsBody>().Endpoints.Should()
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
                    EndpointType.Receiver,
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
}
