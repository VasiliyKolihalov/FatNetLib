using System;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Delegates;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Storages;
using Moq;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Core.Tests
{
    public class EventsEmitterTests
    {
        private Mock<IEndpointsInvoker> _endpointsInvoker = null!;
        private EndpointsStorage _endpointsStorage = null!;
        private EventsEmitter _eventsEmitter = null!;

        [SetUp]
        public void SetUp()
        {
            _endpointsStorage = new EndpointsStorage();
            _endpointsInvoker = new Mock<IEndpointsInvoker>();
            _eventsEmitter = new EventsEmitter(_endpointsStorage, _endpointsInvoker.Object);
        }

        [Test]
        public void Emit_CorrectCase_Pass()
        {
            // Arrange
            var route = new Route("correct-route");
            var body = new object[2];
            LocalEndpoint endpoint = ALocalEndpoint(route);
            _endpointsStorage.LocalEndpoints.Add(endpoint);
            _endpointsStorage.LocalEndpoints.Add(endpoint);
            var package = new Package { Route = route, Body = body };

            // Act
            _eventsEmitter.Emit(package);

            // Assert
            _endpointsInvoker.Verify(_ => _.InvokeReceiver(endpoint, package), Times.Exactly(2));
        }

        [Test]
        public void Emit_NullPackage_Throw()
        {
            // Act
            Action act = () => _eventsEmitter.Emit(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("Value cannot be null. (Parameter 'package')");
        }

        [Test]
        public void Emit_NullRoute_Throw()
        {
            // Act
            Action act = () => _eventsEmitter.Emit(new Package());

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("Value cannot be null. (Parameter 'route')");
        }

        [Test]
        public void Emit_NoRegisterEndpoint_Throw()
        {
            // Act
            Action act = () => _eventsEmitter.Emit(new Package { Route = new Route("correct-route") });

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage("No event-endpoints registered with route correct-route");
        }

        private static LocalEndpoint ALocalEndpoint(Route route)
        {
            return new LocalEndpoint(
                new Endpoint(
                    route,
                    EndpointType.Receiver,
                    Reliability.ReliableSequenced,
                    false,
                    new PackageSchema(),
                    new PackageSchema()),
                new Mock<ReceiverDelegate>().Object);
        }
    }
}
