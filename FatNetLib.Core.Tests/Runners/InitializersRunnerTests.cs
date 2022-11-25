using System;
using System.Collections.Generic;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Runners;
using Kolyhalov.FatNetLib.Core.Storages;
using Moq;
using NUnit.Framework;
using static Moq.Times;

namespace Kolyhalov.FatNetLib.Core.Tests.Runners
{
    public class InitializersRunnerTests
    {
        private const int ServerPeerId = 0;
        private readonly Route _exchangeInitializersRoute = new Route("fat-net-lib/initializers/exchange");
        private InitializersRunner _runner = null!;
        private Mock<ICourier> _courier = null!;
        private IEndpointsStorage _endpointsStorage = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _courier = new Mock<ICourier>();
            _endpointsStorage = new EndpointsStorage();
            var context = new Mock<IDependencyContext>();
            _runner = new InitializersRunner(_courier.Object, _endpointsStorage, context.Object);
        }

        [Test]
        public void Run_CorrectConfiguration_CallInitializers()
        {
            // Arrange
            _endpointsStorage.LocalEndpoints.Add(ALocalInitializer("test/client/init/endpoint"));
            _courier.Setup(_ => _.Send(It.IsAny<Package>()))
                .Returns(new Package
                {
                    Body = new EndpointsBody
                    {
                        Endpoints = new List<Endpoint> { AnInitializer("test/server/init/endpoint") }
                    }
                });

            // Act
            _runner.Run();

            // Assert
            _endpointsStorage.RemoteEndpoints[ServerPeerId][0].Route
                .Should().BeEquivalentTo(_exchangeInitializersRoute);
            _endpointsStorage.RemoteEndpoints[ServerPeerId][1].Route
                .Should().BeEquivalentTo(new Route("test/server/init/endpoint"));
            _endpointsStorage.LocalEndpoints[0].EndpointData.Route
                .Should().BeEquivalentTo(new Route("test/client/init/endpoint"));
            _courier.Verify(
                _ => _.Send(It.Is<Package>(package =>
                    package.Route!.Equals(_exchangeInitializersRoute))),
                Once);
            _courier.Verify(
                _ => _.Send(It.Is<Package>(package =>
                    package.Route!.Equals(new Route("test/server/init/endpoint")))),
                Once);
        }

        private static LocalEndpoint ALocalInitializer(string route)
        {
            return new LocalEndpoint(
                AnInitializer(route),
                action: new Func<Package>(() => new Package()));
        }

        private static Endpoint AnInitializer(string route)
        {
            return new Endpoint(
                new Route(route),
                EndpointType.Initializer,
                Reliability.ReliableSequenced,
                requestSchemaPatch: new PackageSchema(),
                responseSchemaPatch: new PackageSchema());
        }
    }
}
