using System;
using System.Collections.Generic;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Runners;
using Kolyhalov.FatNetLib.Core.Storages;
using Moq;
using NUnit.Framework;
using static Moq.Times;

namespace Kolyhalov.FatNetLib.Core.Tests.Runners
{
    public class InitialEndpointsRunnerTests
    {
        private const int ServerPeerId = 0;
        private readonly Route _initialExchangeEndpointsRoute = new Route("fat-net-lib/init-endpoints/exchange");
        private InitialEndpointsRunner _runner = null!;
        private Mock<ICourier> _courier = null!;
        private IEndpointsStorage _endpointsStorage = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _courier = new Mock<ICourier>();
            _endpointsStorage = new EndpointsStorage();
            var context = new Mock<IDependencyContext>();
            _runner = new InitialEndpointsRunner(_courier.Object, _endpointsStorage, context.Object);
        }

        [Test]
        public void Run_CorrectConfiguration_CallInitialEndpoints()
        {
            // Arrange
            _endpointsStorage.LocalEndpoints.Add(AnInitialLocalEndpoint("test/client/init/endpoint"));
            _courier.Setup(_ => _.SendPackage(It.IsAny<Package>()))
                .Returns(new Package
                {
                    Body = new EndpointsBody
                    {
                        Endpoints = new List<Endpoint> { AnInitialEndpoint("test/server/init/endpoint") }
                    }
                });

            // Act
            _runner.Run();

            // Assert
            _endpointsStorage.RemoteEndpoints[ServerPeerId][0].Route
                .Should().BeEquivalentTo(_initialExchangeEndpointsRoute);
            _endpointsStorage.RemoteEndpoints[ServerPeerId][1].Route
                .Should().BeEquivalentTo(new Route("test/server/init/endpoint"));
            _endpointsStorage.LocalEndpoints[0].EndpointData.Route
                .Should().BeEquivalentTo(new Route("test/client/init/endpoint"));
            _courier.Verify(
                _ => _.SendPackage(It.Is<Package>(package =>
                    package.Route!.Equals(_initialExchangeEndpointsRoute))),
                Once);
            _courier.Verify(
                _ => _.SendPackage(It.Is<Package>(package =>
                    package.Route!.Equals(new Route("test/server/init/endpoint")))),
                Once);
        }

        private static LocalEndpoint AnInitialLocalEndpoint(string route)
        {
            return new LocalEndpoint(
                AnInitialEndpoint(route),
                methodDelegate: new Func<Package>(() => new Package()));
        }

        private static Endpoint AnInitialEndpoint(string route)
        {
            return new Endpoint(
                new Route(route),
                EndpointType.Exchanger,
                Reliability.ReliableSequenced,
                isInitial: true,
                requestSchemaPatch: new PackageSchema(),
                responseSchemaPatch: new PackageSchema());
        }
    }
}
