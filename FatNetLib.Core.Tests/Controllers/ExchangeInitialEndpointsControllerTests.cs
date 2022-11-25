using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture.NUnit3;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Controllers.Server;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Storages;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Core.Tests.Controllers
{
    public class ExchangeInitialEndpointsControllerTests
    {
        private IEndpointsStorage _endpointsStorage = null!;
        private ExchangeInitialEndpointsController _controller = null!;

        [SetUp]
        public void SetUp()
        {
            _endpointsStorage = new EndpointsStorage();
            _controller = new ExchangeInitialEndpointsController(_endpointsStorage);
        }

        [Test, AutoData]
        public void ExchangeInitEndpoints_EndpointsPackage_WriteRemoteAndReturnLocalEndpoints(int peerId)
        {
            // Arrange
            List<Endpoint> endpoints = SomeEndpoints()
                .Where(_ => _.EndpointType is EndpointType.Initial)
                .ToList();
            var requestPackage = new Package
            {
                Body = new EndpointsBody { Endpoints = endpoints },
                FromPeerId = peerId
            };
            RegisterLocalEndpoints(_endpointsStorage);
            _endpointsStorage.LocalEndpoints.Add(GetInitialEndpointsAsEndpoint());

            // Act
            Package responsePackage = _controller.ExchangeInitialEndpoints(requestPackage);

            // Assert
            _endpointsStorage.RemoteEndpoints[peerId].Should().BeEquivalentTo(endpoints);
            responsePackage.GetBodyAs<EndpointsBody>().Endpoints.Should()
                .BeEquivalentTo(endpoints);
        }

        private static LocalEndpoint GetInitialEndpointsAsEndpoint()
        {
            var endpoint = new Endpoint(
                new Route("fat-net-lib/init-endpoints/exchange"),
                EndpointType.Initial,
                Reliability.ReliableOrdered,
                requestSchemaPatch: new PackageSchema(),
                responseSchemaPatch: new PackageSchema());
            return new LocalEndpoint(endpoint, action: new Func<Package>(() => new Package()));
        }

        private static IEnumerable<Endpoint> SomeEndpoints()
        {
            return new List<Endpoint>
            {
                new Endpoint(
                    new Route("test-route1"),
                    EndpointType.Initial,
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
