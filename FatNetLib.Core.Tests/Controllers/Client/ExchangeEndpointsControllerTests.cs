using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture.NUnit3;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Controllers.Client;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Storages;
using Moq;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Core.Tests.Controllers.Client
{
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
            responsePackage.GetBodyAs<EndpointsBody>().Endpoints.Should()
                .BeEquivalentTo(_endpointsStorage.LocalEndpoints
                    .Select(_ => _.EndpointData)
                    .Where(_ => _.IsInitial == false));
        }

        private static List<Endpoint> SomeEndpoints()
        {
            var endpointType = It.IsAny<EndpointType>();
            var reliability = It.IsAny<Reliability>();

            return new List<Endpoint>
            {
                new Endpoint(
                    new Route("test-route1"),
                    endpointType,
                    reliability,
                    isInitial: false,
                    requestSchemaPatch: new PackageSchema(),
                    responseSchemaPatch: new PackageSchema()),
                new Endpoint(
                    new Route("test-route2"),
                    endpointType,
                    reliability,
                    isInitial: true,
                    requestSchemaPatch: new PackageSchema(),
                    responseSchemaPatch: new PackageSchema())
            };
        }

        private static List<LocalEndpoint> SomeLocalEndpoints()
        {
            return SomeEndpoints()
                .Select(endpoint => new LocalEndpoint(endpoint, methodDelegate: new Action(() => { })))
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
