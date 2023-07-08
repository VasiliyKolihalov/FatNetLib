/*
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.NUnit3;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Components;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Couriers;
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
using static Kolyhalov.FatNetLib.Core.Models.EndpointType;
using static Moq.Times;

namespace Kolyhalov.FatNetLib.Core.Tests.Couriers
{
    public class CourierTests
    {
        private EndpointsStorage _endpointsStorage = null!;
        private Mock<ISendingNetPeer> _peer = null!;
        private Mock<IResponsePackageMonitor> _responsePackageMonitor = null!;
        private Mock<IMiddlewaresRunner> _sendingMiddlewaresRunner = null!;
        private Mock<ILogger> _logger = null!;
        private Mock<IEndpointsInvoker> _endpointsInvoker = null!;

        private List<INetPeer> _connectedPeers = null!;
        private ICourier _courier = null!;

        private int PeerId => _peer.Object.Id;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _peer = new Mock<ISendingNetPeer>();
            _peer.Setup(peer => peer.Id)
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

            _courier = new TestCourier(
                _connectedPeers,
                _endpointsStorage,
                _responsePackageMonitor.Object,
                _sendingMiddlewaresRunner.Object,
                _endpointsInvoker.Object,
                _logger.Object);
        }

        [Test]
        public async Task SendAsync_NullPackage_Throw()
        {
            // Act
            Func<Task> action = async () => await _courier.SendAsync(package: null!);

            // Assert
            await action.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'package')");
        }

        [Test]
        public async Task SendAsync_NullRoute_Throw()
        {
            // Act
            Func<Task> action = async () => await _courier.SendAsync(new Package { Route = null });

            // Assert
            await action.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'Route')");
        }

        [Test]
        public async Task SendAsync_NullReceivingPeer_Throw()
        {
            // Act
            Func<Task> act = async () =>
                await _courier.SendAsync(new Package { Route = new Route("correct-route"), Receiver = null });

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'Receiver')");
        }

        [Test]
        public async Task SendAsync_NotFoundEndpoint_Throw()
        {
            // Arrange
            _endpointsStorage.RemoteEndpoints[PeerId] = new List<Endpoint>();
            _connectedPeers.Add(_peer.Object);

            // Act
            Func<Task> action = async () =>
                await _courier.SendAsync(new Package
                {
                    Route = new Route("correct-route"),
                    Receiver = _peer.Object
                });

            // Assert
            await action.Should().ThrowAsync<FatNetLibException>().WithMessage("Remote endpoint not found");
        }

        [Test]
        public async Task SendAsync_EventEndpoint_Throw()
        {
            // Arrange
            var route = new Route("correct-route");
            _endpointsStorage.RemoteEndpoints[PeerId] = new List<Endpoint> { AnEndpoint(route, EventListener) };
            _connectedPeers.Add(_peer.Object);

            // Act
            Func<Task> action = async () => await _courier.SendAsync(new Package
            {
                Route = route,
                Receiver = _peer.Object
            });

            // Assert
            await action.Should().ThrowAsync<FatNetLibException>()
                .WithMessage("Cannot call event listener endpoint over the network");
        }

        [Test]
        public async Task SendAsync_ToReceivingPeer_SendAndReturnNull()
        {
            // Arrange
            RegisterEndpoint(Consumer);
            var package = new Package
            {
                Route = new Route("correct-route"),
                Receiver = _peer.Object,
                ExchangeId = default
            };

            // Act
            Package? result = await _courier.SendAsync(package);

            // Assert
            Assert.AreEqual(null, result);
            _peer.Verify(_ => _.Send(package));
        }

        [Test]
        public async Task SendAsync_ToReceivingPeer_SendingMiddlewareRunnerCalled()
        {
            // Arrange
            RegisterEndpoint(Consumer);
            var package = new Package
            {
                Route = new Route("correct-route"),
                Receiver = _peer.Object,
                ExchangeId = default
            };

            // Act
            await _courier.SendAsync(package);

            // Assert
            _sendingMiddlewaresRunner.Verify(runner => runner.Process(package), Once);
            _sendingMiddlewaresRunner.VerifyNoOtherCalls();
        }

        [Test]
        public async Task SendAsync_ToReceivingPeerGettingErrorResponse_Pass()
        {
            // Arrange
            RegisterEndpoint(Consumer);
            var package = new Package
            {
                Route = new Route("correct-route"),
                Receiver = _peer.Object,
                ExchangeId = default
            };
            _responsePackageMonitor.Setup(_ => _.WaitAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(new Package()));

            // Act
            Func<Task> action = async () => await _courier.SendAsync(package);

            // Assert
            await action.Should().NotThrowAsync();
        }

        [Test]
        public async Task SendAsync_ToExchangerWithoutExchangeId_GenerateExchangeId()
        {
            // Arrange
            RegisterEndpoint(Exchanger);
            var requestPackage = new Package
            {
                Route = new Route("correct-route"),
                Receiver = _peer.Object
            };
            _responsePackageMonitor.Setup(_ => _.WaitAsync(It.IsAny<Guid>()))
                .Returns<Guid>(exchangeId => Task.FromResult(new Package
                {
                    ExchangeId = exchangeId
                }));

            // Act
            Package? actualResponsePackage = await _courier.SendAsync(requestPackage);

            // Assert
            actualResponsePackage!.ExchangeId.Should().NotBeEmpty();
        }

        [Test]
        public async Task SendAsync_ToExchanger_WaitAndReturnResponsePackage()
        {
            // Arrange
            RegisterEndpoint(Exchanger);
            var requestPackage = new Package
            {
                Route = new Route("correct-route"),
                ExchangeId = Guid.NewGuid(),
                Receiver = _peer.Object
            };
            var expectedResponsePackage = new Package();
            _responsePackageMonitor.Setup(_ => _.WaitAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(expectedResponsePackage));
            // Act
            Package? actualResponsePackage = await _courier.SendAsync(requestPackage);

            // Assert
            actualResponsePackage.Should().Be(expectedResponsePackage);
            _peer.Verify(_ => _.Send(requestPackage));
            _responsePackageMonitor.Verify(
                m => m.WaitAsync(It.IsAny<Guid>()), Once);
            _responsePackageMonitor.Verify(m => m.WaitAsync(
                It.Is<Guid>(exchangeId => exchangeId == requestPackage.ExchangeId)));
        }

        [Test]
        public async Task SendAsync_ToExchanger_SendingMiddlewareRunnerCalled()
        {
            // Arrange
            RegisterEndpoint(Exchanger);
            var requestPackage = new Package
            {
                Route = new Route("correct-route"),
                ExchangeId = Guid.NewGuid(),
                Receiver = _peer.Object
            };
            _responsePackageMonitor.Setup(_ => _.WaitAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(new Package()));

            // Act
            await _courier.SendAsync(requestPackage);

            // Assert
            _sendingMiddlewaresRunner.Verify(_ => _.Process(requestPackage), Once);
            _sendingMiddlewaresRunner.VerifyNoOtherCalls();
        }

        [Test]
        public async Task SendAsync_ToExchangerGettingErrorResponse_Throw()
        {
            // Arrange
            RegisterEndpoint(Exchanger);
            var requestPackage = new Package
            {
                Route = new Route("correct-route"),
                ExchangeId = Guid.NewGuid(),
                Receiver = _peer.Object
            };
            _responsePackageMonitor.Setup(_ => _.WaitAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(new Package { Error = "test-error" }));

            // Act
            Func<Task> act = async () => await _courier.SendAsync(requestPackage);

            // Assert
            await act.Should().ThrowAsync<ErrorResponseFatNetLibException>()
                .WithMessage("Peer responded with error. Error=test-error");
        }

        [Test]
        public async Task SendAsync_ToInitializerWithoutExchangeId_GenerateExchangeId()
        {
            // Arrange
            RegisterEndpoint(Initializer);
            var requestPackage = new Package
            {
                Route = new Route("correct-route"),
                Receiver = _peer.Object
            };
            _responsePackageMonitor.Setup(_ => _.WaitAsync(It.IsAny<Guid>()))
                .Returns<Guid>(exchangeId => Task.FromResult(new Package { ExchangeId = exchangeId }));

            // Act
            Package? actualResponsePackage = await _courier.SendAsync(requestPackage);

            // Assert
            actualResponsePackage!.ExchangeId.Should().NotBeEmpty();
        }

        [Test]
        public async Task SendAsync_ToInitializer_WaitAndReturnResponsePackage()
        {
            // Arrange
            RegisterEndpoint(Initializer);
            var requestPackage = new Package
            {
                Route = new Route("correct-route"),
                ExchangeId = Guid.NewGuid(),
                Receiver = _peer.Object
            };
            var expectedResponsePackage = new Package();
            _responsePackageMonitor.Setup(_ => _.WaitAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(expectedResponsePackage));
            // Act
            Package? actualResponsePackage = await _courier.SendAsync(requestPackage);

            // Assert
            actualResponsePackage.Should().Be(expectedResponsePackage);
            _peer.Verify(_ => _.Send(requestPackage));
            _responsePackageMonitor.Verify(
                m => m.WaitAsync(
                    It.IsAny<Guid>()),
                Once);
            _responsePackageMonitor.Verify(m => m.WaitAsync(
                It.Is<Guid>(exchangeId => exchangeId == requestPackage.ExchangeId)));
        }

        [Test]
        public async Task SendAsync_ToInitializerGettingErrorResponse_Throw()
        {
            // Arrange
            RegisterEndpoint(Initializer);
            var requestPackage = new Package
            {
                Route = new Route("correct-route"),
                ExchangeId = Guid.NewGuid(),
                Receiver = _peer.Object
            };
            _responsePackageMonitor.Setup(_ => _.WaitAsync(It.IsAny<Guid>()))
                .Returns(() => Task.FromResult(new Package { Error = "test-error" }));

            // Act
            Func<Task> action = async () => await _courier.SendAsync(requestPackage);

            // Assert
            await action.Should().ThrowAsync<ErrorResponseFatNetLibException>()
                .WithMessage("Peer responded with error. Error=test-error");
        }

        [Test, AutoData]
        public async Task EmitEventAsync_CorrectCase_Pass(object body)
        {
            // Arrange
            var route = new Route("correct-route");
            LocalEndpoint endpoint = ALocalEndpoint(route, EventListener);
            _endpointsStorage.LocalEndpoints.Add(endpoint);
            _endpointsStorage.LocalEndpoints.Add(endpoint);
            var package = new Package { Route = route, Body = body };

            // Act
            await _courier.EmitEventAsync(package);

            // Assert
            _endpointsInvoker.Verify(_ => _.InvokeConsumerAsync(endpoint, package), times: Exactly(2));
        }

        [Test]
        public async Task EmitEventAsync_NullPackage_Throw()
        {
            // Act
            Func<Task> act = async () => await _courier.EmitEventAsync(null!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'package')");
        }

        [Test]
        public async Task EmitEventAsync_NullRoute_Throw()
        {
            // Act
            Func<Task> act = async () => await _courier.EmitEventAsync(new Package { Route = null });

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'route')");
        }

        [Test]
        public async Task EmitEventAsync_NoRegisterEndpoint_CallLogger()
        {
            // Act
            await _courier.EmitEventAsync(new Package { Route = new Route("correct-route") });

            // Assert
            _logger.Verify(_ => _.Debug("No event endpoints registered with route correct-route"));
        }

        [Test]
        public async Task EmitEventAsync_NonEventEndpoint_Pass()
        {
            // Arrange
            var route = new Route("correct-route");
            _endpointsStorage.LocalEndpoints.Add(ALocalEndpoint(route, Initializer));
            var package = new Package { Route = route };

            // Act
            Func<Task> act = async () => await _courier.EmitEventAsync(package);

            // Assert
            await act.Should().ThrowAsync<FatNetLibException>()
                .WithMessage("Cannot emit event to not event listener endpoint");
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

        private void RegisterEndpoint(EndpointType type)
        {
            var endpoint = new Endpoint(
                new Route("correct-route"),
                type,
                Reliability.Sequenced,
                requestSchemaPatch: new PackageSchema(),
                responseSchemaPatch: new PackageSchema());
            _endpointsStorage.RemoteEndpoints[PeerId] = new List<Endpoint> { endpoint };
            _connectedPeers.Add(_peer.Object);
        }
    }

    public class TestCourier : Courier
    {
        public TestCourier(
            IList<INetPeer> connectedPeers,
            IEndpointsStorage endpointsStorage,
            IResponsePackageMonitor responsePackageMonitor,
            IMiddlewaresRunner sendingMiddlewaresRunner,
            IEndpointsInvoker endpointsInvoker,
            ILogger logger)
            : base(
                connectedPeers,
                endpointsStorage,
                responsePackageMonitor,
                sendingMiddlewaresRunner,
                endpointsInvoker,
                logger)
        {
        }
    }
}
*/
