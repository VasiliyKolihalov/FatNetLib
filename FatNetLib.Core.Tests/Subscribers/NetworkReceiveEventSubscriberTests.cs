using System;
using System.Collections.Generic;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Monitors;
using Kolyhalov.FatNetLib.Core.Runners;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Subscribers;
using Kolyhalov.FatNetLib.Core.Tests.Utils;
using Kolyhalov.FatNetLib.Core.Wrappers;
using LiteNetLib.Utils;
using Moq;
using NUnit.Framework;
using static System.Text.Encoding;
using static Moq.Times;

namespace Kolyhalov.FatNetLib.Core.Tests.Subscribers
{
    public class NetworkReceiveEventSubscriberTests
    {
        private readonly Mock<DependencyContext> _context = new Mock<DependencyContext>();
        private readonly PackageSchema _defaultSchema = new PackageSchema();
        private INetworkReceiveEventSubscriber _subscriber = null!;
        private Mock<IResponsePackageMonitor> _responsePackageMonitor = null!;
        private Mock<INetPeer> _netPeer = null!;
        private Mock<IMiddlewaresRunner> _sendingMiddlewaresRunner = null!;
        private Mock<IMiddlewaresRunner> _receivingMiddlewaresRunner = null!;
        private Mock<IEndpointsInvoker> _endpointsInvoker = null!;
        private IEndpointsStorage _endpointsStorage = null!;
        private Mock<ICourier> _courier = null!;

        private int PeerId => _netPeer.Object.Id;

        [SetUp]
        public void SetUp()
        {
            _responsePackageMonitor = new Mock<IResponsePackageMonitor>();
            _sendingMiddlewaresRunner = new Mock<IMiddlewaresRunner>();
            _receivingMiddlewaresRunner = new Mock<IMiddlewaresRunner>();
            _endpointsInvoker = new Mock<IEndpointsInvoker>();
            _endpointsStorage = new EndpointsStorage();
            _netPeer = new Mock<INetPeer>();
            _netPeer.Setup(netPeer => netPeer.Id)
                .Returns(0);
            IList<INetPeer> connectedPeers = new List<INetPeer> { _netPeer.Object };
            _courier = new Mock<ICourier>();

            _subscriber = new NetworkReceiveEventSubscriber(
                _responsePackageMonitor.Object,
                _receivingMiddlewaresRunner.Object,
                _defaultSchema,
                _context.Object,
                _endpointsStorage,
                _endpointsInvoker.Object,
                _sendingMiddlewaresRunner.Object,
                connectedPeers,
                new Route("last/initializer/handle"),
                _courier.Object);
        }

        [Test]
        public void Handle_Receiver_Handle()
        {
            // Arrange
            _receivingMiddlewaresRunner.Setup(_ => _.Process(It.IsAny<Package>()))
                .Callback(delegate(Package package) { package.Route = new Route("test/route"); });
            _endpointsStorage.LocalEndpoints.Add(AReceiver());

            // Act
            _subscriber.Handle(_netPeer.Object, ANetDataReader(), Reliability.ReliableOrdered);

            // Assert
            _sendingMiddlewaresRunner.VerifyNoOtherCalls();
            _receivingMiddlewaresRunner.Verify(_ => _.Process(It.IsAny<Package>()), Once);
            _responsePackageMonitor.VerifyNoOtherCalls();
            _endpointsInvoker.Verify(_ => _.InvokeReceiver(It.IsAny<LocalEndpoint>(), It.IsAny<Package>()));
            _endpointsInvoker.VerifyNoOtherCalls();
            _netPeer.Verify(_ => _.Send(It.IsAny<Package>()), Never);
        }

        [Test]
        public void Handle_ExchangerRequest_Handle()
        {
            // Arrange
            _receivingMiddlewaresRunner.Setup(_ => _.Process(It.IsAny<Package>()))
                .Callback(delegate(Package package) { package.Route = new Route("test/route"); });
            _endpointsStorage.LocalEndpoints.Add(AnExchanger());
            _endpointsInvoker.Setup(_ => _.InvokeExchanger(It.IsAny<LocalEndpoint>(), It.IsAny<Package>()))
                .Returns(new Package());

            // Act
            _subscriber.Handle(_netPeer.Object, ANetDataReader(), Reliability.ReliableOrdered);

            // Assert
            _receivingMiddlewaresRunner.Verify(_ => _.Process(It.IsAny<Package>()), Once);
            _receivingMiddlewaresRunner.Verify(_ => _.Process(It.IsAny<Package>()), Once);
            _responsePackageMonitor.VerifyNoOtherCalls();
            _endpointsInvoker.Verify(_ => _.InvokeExchanger(It.IsAny<LocalEndpoint>(), It.IsAny<Package>()));
            _endpointsInvoker.VerifyNoOtherCalls();
            _netPeer.Verify(_ => _.Send(It.IsAny<Package>()), Once);
        }

        [Test]
        public void Handle_InitialRequest_Handle()
        {
            // Arrange
            _receivingMiddlewaresRunner.Setup(_ => _.Process(It.IsAny<Package>()))
                .Callback(delegate(Package package) { package.Route = new Route("test/route"); });
            _endpointsStorage.LocalEndpoints.Add(AnInitializer);
            _endpointsInvoker.Setup(_ => _.InvokeExchanger(It.IsAny<LocalEndpoint>(), It.IsAny<Package>()))
                .Returns(new Package());

            // Act
            _subscriber.Handle(_netPeer.Object, ANetDataReader(), Reliability.ReliableOrdered);

            // Assert
            _receivingMiddlewaresRunner.Verify(_ => _.Process(It.IsAny<Package>()), Once);
            _receivingMiddlewaresRunner.Verify(_ => _.Process(It.IsAny<Package>()), Once);
            _responsePackageMonitor.VerifyNoOtherCalls();
            _endpointsInvoker.Verify(_ => _.InvokeExchanger(It.IsAny<LocalEndpoint>(), It.IsAny<Package>()));
            _endpointsInvoker.VerifyNoOtherCalls();
            _netPeer.Verify(_ => _.Send(It.IsAny<Package>()), Once);
            _courier.VerifyNoOtherCalls();
        }

        [Test]
        public void Handle_LastInitializer_EmitEvent()
        {
            // Arrange
            _receivingMiddlewaresRunner.Setup(_ => _.Process(It.IsAny<Package>()))
                .Callback(delegate(Package package) { package.Route = new Route("last/initializer/handle"); });
            _endpointsStorage.LocalEndpoints.Add(LastInitializer);
            _endpointsInvoker.Setup(_ => _.InvokeExchanger(It.IsAny<LocalEndpoint>(), It.IsAny<Package>()))
                .Returns(new Package());

            // Act
            _subscriber.Handle(_netPeer.Object, ANetDataReader(), Reliability.ReliableOrdered);

            // Assert
            _endpointsInvoker.Verify(_ => _.InvokeExchanger(It.IsAny<LocalEndpoint>(), It.IsAny<Package>()));
            _endpointsInvoker.VerifyNoOtherCalls();
            _courier.Verify(_ => _.EmitEvent(It.IsAny<Package>()), Once());
            _courier.VerifyNoOtherCalls();
        }

        [Test]
        public void Handle_InitializerAfterLastInitializer_Throw()
        {
            // Arrange
            _receivingMiddlewaresRunner.Setup(_ => _.Process(It.IsAny<Package>()))
                .Callback(delegate(Package package1)
                {
                    package1.Route = new Route("last/initializer/handle");
                    _receivingMiddlewaresRunner.Setup(_ => _.Process(It.IsAny<Package>()))
                        .Callback(delegate(Package package2) { package2.Route = new Route("test/route"); });
                });

            _endpointsStorage.LocalEndpoints.Add(LastInitializer);
            _endpointsStorage.LocalEndpoints.Add(AnInitializer);
            _endpointsInvoker.Setup(_ => _.InvokeExchanger(It.IsAny<LocalEndpoint>(), It.IsAny<Package>()))
                .Returns(new Package());

            _subscriber.Handle(_netPeer.Object, ANetDataReader(), Reliability.ReliableOrdered);

            // Act
            Action act = () => _subscriber.Handle(_netPeer.Object, ANetDataReader(), Reliability.ReliableOrdered);

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage("Last initializer was already called");
        }

        [Test]
        public void Handle_Response_Handle()
        {
            // Arrange
            _receivingMiddlewaresRunner.Setup(_ => _.Process(It.IsAny<Package>()))
                .Callback<Package>(package => package.IsResponse = true);

            // Act
            _subscriber.Handle(_netPeer.Object, ANetDataReader(), Reliability.ReliableOrdered);

            // Assert
            _sendingMiddlewaresRunner.VerifyNoOtherCalls();
            _receivingMiddlewaresRunner.Verify(_ => _.Process(It.IsAny<Package>()), Once);
            _responsePackageMonitor.Verify(_ => _.Pulse(It.IsAny<Package>()), Once);
            _endpointsInvoker.VerifyNoOtherCalls();
            _netPeer.Verify(_ => _.Send(It.IsAny<Package>()), Never);
        }

        [Test]
        public void Handle_UnknownEndpoint_Throw()
        {
            // Arrange
            _receivingMiddlewaresRunner.Setup(_ => _.Process(It.IsAny<Package>()))
                .Callback(delegate(Package package) { package.Route = new Route("another/test/route"); });
            _endpointsStorage.LocalEndpoints.Add(AReceiver());

            // Act
            Action act = () => _subscriber.Handle(_netPeer.Object, ANetDataReader(), Reliability.ReliableOrdered);

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage("Package from 0 pointed to a non-existent endpoint. Route: another/test/route");
        }

        [Test]
        public void Handle_WrongReliability_Throw()
        {
            // Arrange
            _receivingMiddlewaresRunner.Setup(_ => _.Process(It.IsAny<Package>()))
                .Callback(delegate(Package package) { package.Route = new Route("test/route"); });
            _endpointsStorage.LocalEndpoints.Add(AReceiver());

            // Act
            Action act = () => _subscriber.Handle(_netPeer.Object, ANetDataReader(), Reliability.Unreliable);

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage("Package from 0 came with the wrong type of reliability");
        }

        [Test]
        public void Handle_CorrectEvent_BuildCorrectReceivedPackage()
        {
            // Arrange
            Package receivedPackage = null!;
            _receivingMiddlewaresRunner.Setup(_ => _.Process(It.IsAny<Package>()))
                .Callback(delegate(Package package)
                {
                    receivedPackage = package;
                    package.Route = new Route("test/route");
                });
            _endpointsStorage.LocalEndpoints.Add(AReceiver());

            // Act
            _subscriber.Handle(_netPeer.Object, ANetDataReader(), Reliability.ReliableOrdered);

            // Assert
            receivedPackage.Serialized.Should().BeEquivalentToUtf8("some-json-package");
            receivedPackage.Schema.Should().BeEquivalentTo(_defaultSchema);
            receivedPackage.Schema.Should().NotBeSameAs(_defaultSchema);
            receivedPackage.Context.Should().Be(_context.Object);
            receivedPackage.FromPeerId.Should().Be(PeerId);
            receivedPackage.Reliability.Should().Be(Reliability.ReliableOrdered);
        }

        [Test]
        public void Handle_CorrectEvent_BuildCorrectPackageToSend()
        {
            // Arrange
            Package packageToSend = null!;
            _sendingMiddlewaresRunner.Setup(_ => _.Process(It.IsAny<Package>()))
                .Callback(delegate(Package package) { packageToSend = package; });
            _receivingMiddlewaresRunner.Setup(_ => _.Process(It.IsAny<Package>()))
                .Callback(delegate(Package package) { package.Route = new Route("test/route"); });
            _endpointsStorage.LocalEndpoints.Add(AnExchanger());
            _endpointsInvoker.Setup(_ => _.InvokeExchanger(It.IsAny<LocalEndpoint>(), It.IsAny<Package>()))
                .Returns(new Package());

            // Act
            _subscriber.Handle(_netPeer.Object, ANetDataReader(), Reliability.ReliableOrdered);

            // Assert
            packageToSend.IsResponse.Should().BeTrue();
        }

        private static LocalEndpoint ALocalEndpoint(EndpointType endpointType)
        {
            return new LocalEndpoint(
                new Endpoint(
                    new Route("test/route"),
                    endpointType,
                    Reliability.ReliableOrdered,
                    requestSchemaPatch: new PackageSchema(),
                    responseSchemaPatch: new PackageSchema()),
                action: new Func<Package>(() => new Package()));
        }

        private static LocalEndpoint AReceiver() => ALocalEndpoint(EndpointType.Receiver);

        private static LocalEndpoint AnExchanger() => ALocalEndpoint(EndpointType.Exchanger);

        private static LocalEndpoint AnInitializer => ALocalEndpoint(EndpointType.Initializer);

        private static LocalEndpoint LastInitializer => new LocalEndpoint(
            new Endpoint(
                new Route("last/initializer/handle"),
                EndpointType.Initializer,
                Reliability.ReliableOrdered,
                requestSchemaPatch: new PackageSchema(),
                responseSchemaPatch: new PackageSchema()),
            action: new Func<Package>(() => new Package()));

        private static NetDataReader ANetDataReader()
        {
            var netDataWriter = new NetDataWriter();
            netDataWriter.Put(UTF8.GetBytes("some-json-package"));
            return new NetDataReader(netDataWriter);
        }
    }
}
