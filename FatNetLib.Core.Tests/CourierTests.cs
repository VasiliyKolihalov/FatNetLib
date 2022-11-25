using System;
using System.Collections.Generic;
using AutoFixture;
using AutoFixture.NUnit3;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Monitors;
using Kolyhalov.FatNetLib.Core.Runners;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Wrappers;
using Moq;
using NUnit.Framework;
using static System.Text.Encoding;
using static Moq.Times;

namespace Kolyhalov.FatNetLib.Core.Tests
{
    public class CourierTests
    {
        private EndpointsStorage _endpointsStorage = null!;
        private Mock<INetPeer> _netPeer = null!;
        private Mock<IResponsePackageMonitor> _responsePackageMonitor = null!;
        private Mock<IMiddlewaresRunner> _sendingMiddlewaresRunner = null!;
        private Mock<ILogger> _logger = null!;
        private Mock<IEndpointsInvoker> _endpointsInvoker = null!;

        private List<INetPeer> _connectedPeers = null!;
        private ICourier _courier = null!;

        private int PeerId => _netPeer.Object.Id;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _netPeer = new Mock<INetPeer>();
            _netPeer.Setup(netPeer => netPeer.Id)
                .Returns(new Fixture().Create<int>());
        }

        [SetUp]
        public void SetUp()
        {
            _endpointsStorage = new EndpointsStorage();
            _responsePackageMonitor = new Mock<IResponsePackageMonitor>();
            _logger = new Mock<ILogger>();
            _endpointsInvoker = new Mock<IEndpointsInvoker>();
            _sendingMiddlewaresRunner = AMiddlewareRunner();

            _connectedPeers = new List<INetPeer>();

            _courier = new Courier(
                _connectedPeers,
                _endpointsStorage,
                _responsePackageMonitor.Object,
                _sendingMiddlewaresRunner.Object,
                _endpointsInvoker.Object,
                _logger.Object);
        }

        [Test]
        public void Send_NullPackage_Throw()
        {
            // Act
            void Action() => _courier.Send(package: null!);

            // Assert
            Assert.That(
                Action,
                Throws.TypeOf<ArgumentNullException>().With.Message
                    .Contains("Value cannot be null. (Parameter 'package')"));
        }

        [Test]
        public void Send_NullRoute_Throw()
        {
            // Act
            Action act = () => _courier.Send(new Package());

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'Route')");
        }

        [Test]
        public void Send_NullReceivingPeer_Throw()
        {
            // Act
            Action act = () =>
                _courier.Send(new Package { Route = new Route("correct-route"), ToPeerId = null });

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'ToPeerId')");
        }

        [Test]
        public void Send_NotFoundReceivingPeer_Throw()
        {
            // Act
            Action act = () => _courier.Send(new Package { Route = new Route("correct-route"), ToPeerId = 42 });

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage("Receiving peer not found");
        }

        [Test]
        public void Send_NotFoundEndpoint_Throw()
        {
            // Arrange
            _endpointsStorage.RemoteEndpoints[PeerId] = new List<Endpoint>();
            _connectedPeers.Add(_netPeer.Object);

            // Act
            void Action() =>
                _courier.Send(new Package { Route = new Route("correct-route"), ToPeerId = PeerId });

            // Assert
            Assert.That(Action, Throws.TypeOf<FatNetLibException>().With.Message.Contains("Endpoint not found"));
        }

        [Test]
        public void Send_EventEndpoint_Throw()
        {
            // Arrange
            var route = new Route("correct-route");
            _endpointsStorage.RemoteEndpoints[PeerId] = new List<Endpoint> { AnEndpoint(route, EndpointType.Event) };
            _connectedPeers.Add(_netPeer.Object);

            // Act
            Action act = () => _courier.Send(new Package { Route = route, ToPeerId = PeerId });

            // Assert
            act.Should().Throw<FatNetLibException>().WithMessage("Cannot call event-endpoint over the network");
        }

        [Test]
        public void Send_ToExchangerWithoutExchangeId_GenerateExchangeId()
        {
            // Arrange
            RegisterEndpoint();
            var requestPackage = new Package
            {
                Route = new Route("correct-route"),
                ExchangeId = Guid.Empty,
                ToPeerId = PeerId
            };
            _responsePackageMonitor.Setup(m => m.Wait(It.IsAny<Guid>()))
                .Returns(new Func<Guid, Package>(exchangeId => new Package { ExchangeId = exchangeId }));

            // Act
            Package? actualResponsePackage = _courier.Send(requestPackage);

            // Assert
            actualResponsePackage!.ExchangeId.Should().NotBeEmpty();
        }

        [Test]
        public void Send_ToInitialWithoutExchangeId_GenerateExchangeId()
        {
            // Arrange
            var route = new Route("correct-route");
            _endpointsStorage.RemoteEndpoints[PeerId] = new List<Endpoint>
            {
                AnEndpoint(route, EndpointType.Initializer)
            };
            _connectedPeers.Add(_netPeer.Object);
            var requestPackage = new Package
            {
                Route = route,
                ExchangeId = Guid.Empty,
                ToPeerId = PeerId
            };
            _responsePackageMonitor.Setup(m => m.Wait(It.IsAny<Guid>()))
                .Returns(new Func<Guid, Package>(exchangeId => new Package { ExchangeId = exchangeId }));

            // Act
            Package? actualResponsePackage = _courier.Send(requestPackage);

            // Assert
            actualResponsePackage!.ExchangeId.Should().NotBeEmpty();
        }

        [Test]
        public void Send_ToReceivingPeer_SendAndReturnNull()
        {
            // Arrange
            RegisterEndpoint();
            var package = new Package { Route = new Route("correct-route"), ToPeerId = PeerId };

            // Act
            Package? result = _courier.Send(package);

            // Assert
            Assert.AreEqual(null, result);
            _netPeer.Verify(netPeer => netPeer.Send(package));
        }

        [Test]
        public void Send_ToReceivingPeer_SendingMiddlewareRunnerCalled()
        {
            // Arrange
            RegisterEndpoint();
            var package = new Package { Route = new Route("correct-route"), ToPeerId = PeerId };

            // Act
            _courier.Send(package);

            // Assert
            _sendingMiddlewaresRunner.Verify(runner => runner.Process(package), Once);
            _sendingMiddlewaresRunner.VerifyNoOtherCalls();
        }

        [Test]
        public void Send_ToExchanger_WaitAndReturnResponsePackage()
        {
            // Arrange
            RegisterEndpoint();
            var requestPackage = new Package
            {
                Route = new Route("correct-route"),
                ExchangeId = Guid.NewGuid(),
                ToPeerId = PeerId
            };
            var expectedResponsePackage = new Package();
            _responsePackageMonitor.Setup(m => m.Wait(It.IsAny<Guid>()))
                .Returns(expectedResponsePackage);

            // Act
            Package? actualResponsePackage = _courier.Send(requestPackage);

            // Assert
            actualResponsePackage.Should().Be(expectedResponsePackage);
            _netPeer.Verify(netPeer => netPeer.Send(requestPackage));
            _responsePackageMonitor.Verify(m => m.Wait(It.IsAny<Guid>()), Once);
            _responsePackageMonitor.Verify(m => m.Wait(
                It.Is<Guid>(exchangeId => exchangeId == requestPackage.ExchangeId)));
        }

        [Test]
        public void Send_ToInitial_WaitAndReturnResponsePackage()
        {
            // Arrange
            var route = new Route("correct-route");
            _endpointsStorage.RemoteEndpoints[PeerId] = new List<Endpoint>
            {
                AnEndpoint(route, EndpointType.Initializer)
            };
            _connectedPeers.Add(_netPeer.Object);
            var requestPackage = new Package
            {
                Route = new Route("correct-route"),
                ExchangeId = Guid.NewGuid(),
                ToPeerId = PeerId
            };
            var expectedResponsePackage = new Package();
            _responsePackageMonitor.Setup(m => m.Wait(It.IsAny<Guid>()))
                .Returns(expectedResponsePackage);

            // Act
            Package? actualResponsePackage = _courier.Send(requestPackage);

            // Assert
            actualResponsePackage.Should().Be(expectedResponsePackage);
            _netPeer.Verify(netPeer => netPeer.Send(requestPackage));
            _responsePackageMonitor.Verify(m => m.Wait(It.IsAny<Guid>()), Once);
            _responsePackageMonitor.Verify(m => m.Wait(
                It.Is<Guid>(exchangeId => exchangeId == requestPackage.ExchangeId)));
        }

        [Test]
        public void Send_ToExchangingPeer_SendingMiddlewareRunnerCalled()
        {
            // Arrange
            RegisterEndpoint();
            var requestPackage = new Package
            {
                Route = new Route("correct-route"),
                ExchangeId = Guid.NewGuid(),
                ToPeerId = PeerId
            };

            // Act
            _courier.Send(requestPackage);

            // Assert
            _sendingMiddlewaresRunner.Verify(_ => _.Process(requestPackage), Once);
            _sendingMiddlewaresRunner.VerifyNoOtherCalls();
        }

        [Test]
        public void Send_PackageWasNotSerializedByMiddlewares_Throw()
        {
            // Arrange
            RegisterEndpoint();
            var requestPackage = new Package
            {
                Route = new Route("correct-route"),
                ExchangeId = Guid.NewGuid(),
                ToPeerId = PeerId
            };
            _sendingMiddlewaresRunner.Setup(_ => _.Process(It.IsAny<Package>()))
                .Callback<Package>(package => { package.Serialized = null; });

            // Act
            Action act = () => _courier.Send(requestPackage);

            // Assert
            act.Should().Throw<FatNetLibException>().WithMessage("Serialized field is missing");
        }

        [Test, AutoData]
        public void EmitEvent_CorrectCase_Pass(object body)
        {
            // Arrange
            var route = new Route("correct-route");
            LocalEndpoint endpoint = ALocalEndpoint(route, EndpointType.Event);
            _endpointsStorage.LocalEndpoints.Add(endpoint);
            _endpointsStorage.LocalEndpoints.Add(endpoint);
            var package = new Package { Route = route, Body = body };

            // Act
            _courier.EmitEvent(package);

            // Assert
            _endpointsInvoker.Verify(_ => _.InvokeReceiver(endpoint, package), times: Exactly(2));
        }

        [Test]
        public void EmitEvent_NullPackage_Throw()
        {
            // Act
            Action act = () => _courier.EmitEvent(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("Value cannot be null. (Parameter 'package')");
        }

        [Test]
        public void EmitEvent_NullRoute_Throw()
        {
            // Act
            Action act = () => _courier.EmitEvent(new Package());

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("Value cannot be null. (Parameter 'route')");
        }

        [Test]
        public void EmitEvent_NoRegisterEndpoint_CallLogger()
        {
            // Act
            _courier.EmitEvent(new Package { Route = new Route("correct-route") });

            // Assert
            _logger.Verify(_ => _.Debug("No event-endpoints registered with route correct-route"));
        }

        [Test]
        public void EmitEvent_NonEventEndpoint_Pass()
        {
            // Arrange
            var route = new Route("correct-route");
            _endpointsStorage.LocalEndpoints.Add(ALocalEndpoint(route, EndpointType.Initializer));
            var package = new Package { Route = route };

            // Act
            Action act = () => _courier.EmitEvent(package);

            // Assert
            act.Should().Throw<FatNetLibException>().WithMessage("Cannot emit not event endpoint");
        }

        private static Mock<IMiddlewaresRunner> AMiddlewareRunner()
        {
            var middlewareRunner = new Mock<IMiddlewaresRunner>();
            middlewareRunner.Setup(_ => _.Process(It.IsAny<Package>()))
                .Callback<Package>(package => { package.Serialized = UTF8.GetBytes("serialized-package"); });
            return middlewareRunner;
        }

        private static Endpoint AnEndpoint(Route route, EndpointType endpointType)
        {
            return new Endpoint(
                route,
                endpointType,
                Reliability.ReliableSequenced,
                new PackageSchema(),
                new PackageSchema());
        }

        private static LocalEndpoint ALocalEndpoint(Route route, EndpointType endpointType)
        {
            return new LocalEndpoint(
                new Endpoint(
                    route,
                    endpointType,
                    Reliability.ReliableSequenced,
                    new PackageSchema(),
                    new PackageSchema()),
                new Func<Package>(() => new Package()));
        }

        private void RegisterEndpoint()
        {
            var endpoint = new Endpoint(
                new Route("correct-route"),
                EndpointType.Exchanger,
                Reliability.Sequenced,
                requestSchemaPatch: new PackageSchema(),
                responseSchemaPatch: new PackageSchema());
            _endpointsStorage.RemoteEndpoints[PeerId] = new List<Endpoint> { endpoint };
            _connectedPeers.Add(_netPeer.Object);
        }
    }
}
