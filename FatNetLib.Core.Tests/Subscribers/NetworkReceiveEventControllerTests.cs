using System;
using System.Collections.Generic;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Couriers;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Monitors;
using Kolyhalov.FatNetLib.Core.Runners;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Subscribers;
using Kolyhalov.FatNetLib.Core.Wrappers;
using LiteNetLib.Utils;
using Moq;
using NUnit.Framework;
using static System.Text.Encoding;
using static Kolyhalov.FatNetLib.Core.Models.Reliability;
using static Moq.Times;

namespace Kolyhalov.FatNetLib.Core.Tests.Subscribers
{
    public class NetworkReceiveEventControllerTests
    {
        private readonly Mock<DependencyContext> _context = new();
        private readonly PackageSchema _defaultSchema = new();

        private NetworkReceiveEventController _controller = null!;
        private Mock<IResponsePackageMonitor> _responsePackageMonitor = null!;
        private Mock<ISendingNetPeer> _peer = null!;
        private Mock<IMiddlewaresRunner> _sendingMiddlewaresRunner = null!;
        private Mock<IMiddlewaresRunner> _receivingMiddlewaresRunner = null!;
        private Mock<IEndpointsInvoker> _endpointsInvoker = null!;
        private IEndpointsStorage _endpointsStorage = null!;
        private Mock<ICourier> _courier = null!;

        [SetUp]
        public void SetUp()
        {
            _responsePackageMonitor = new Mock<IResponsePackageMonitor>();
            _sendingMiddlewaresRunner = new Mock<IMiddlewaresRunner>();
            _receivingMiddlewaresRunner = new Mock<IMiddlewaresRunner>();
            _endpointsInvoker = new Mock<IEndpointsInvoker>();
            _endpointsStorage = new EndpointsStorage();
            _peer = new Mock<ISendingNetPeer>();
            _peer.Setup(peer => peer.Id)
                .Returns(0);
            _courier = new Mock<ICourier>();

            _controller = new NetworkReceiveEventController(
                _responsePackageMonitor.Object,
                _receivingMiddlewaresRunner.Object,
                _defaultSchema,
                _context.Object,
                _endpointsStorage,
                _endpointsInvoker.Object,
                _sendingMiddlewaresRunner.Object);
        }

        [Test]
        public void Handle_Receiver_Handle()
        {
            // Arrange
            _receivingMiddlewaresRunner.Setup(_ => _.Process(It.IsAny<Package>()))
                .Callback((Package package) =>
                {
                    package.Route = new Route("test/route");
                    package.IsResponse = false;
                });
            _endpointsStorage.LocalEndpoints.Add(AReceiver());

            // Act
            _controller.Handle(APackage(_peer.Object, ANetDataReader(), ReliableOrdered));

            // Assert
            _sendingMiddlewaresRunner.VerifyNoOtherCalls();
            _receivingMiddlewaresRunner.Verify(_ => _.Process(It.IsAny<Package>()), Once);
            _responsePackageMonitor.VerifyNoOtherCalls();
            _endpointsInvoker.Verify(_ => _.InvokeReceiver(It.IsAny<LocalEndpoint>(), It.IsAny<Package>()));
            _endpointsInvoker.VerifyNoOtherCalls();
            _peer.Verify(_ => _.Send(It.IsAny<Package>()), Never);
        }

        [Test]
        public void Handle_Exchanger_Handle()
        {
            // Arrange
            ReceivingMiddlewaresRunnerProcessesPackage();
            _endpointsStorage.LocalEndpoints.Add(AnExchanger());
            _endpointsInvoker.Setup(_ => _.InvokeExchanger(It.IsAny<LocalEndpoint>(), It.IsAny<Package>()))
                .Returns(new Package());

            // Act
            _controller.Handle(APackage(_peer.Object, ANetDataReader(), ReliableOrdered));

            // Assert
            _receivingMiddlewaresRunner.Verify(_ => _.Process(It.IsAny<Package>()), Once);
            _responsePackageMonitor.VerifyNoOtherCalls();
            _endpointsInvoker.Verify(_ => _.InvokeExchanger(It.IsAny<LocalEndpoint>(), It.IsAny<Package>()));
            _endpointsInvoker.VerifyNoOtherCalls();
            _peer.Verify(_ => _.Send(It.IsAny<Package>()), Once);
        }

        [Test]
        public void Handle_Initializer_Handle()
        {
            // Arrange
            ReceivingMiddlewaresRunnerProcessesPackage();
            _endpointsStorage.LocalEndpoints.Add(AnInitializer);
            _endpointsInvoker.Setup(_ => _.InvokeExchanger(It.IsAny<LocalEndpoint>(), It.IsAny<Package>()))
                .Returns(new Package());

            // Act
            _controller.Handle(APackage(_peer.Object, ANetDataReader(), ReliableOrdered));

            // Assert
            _receivingMiddlewaresRunner.Verify(_ => _.Process(It.IsAny<Package>()), Once);
            _receivingMiddlewaresRunner.Verify(_ => _.Process(It.IsAny<Package>()), Once);
            _responsePackageMonitor.VerifyNoOtherCalls();
            _endpointsInvoker.Verify(_ => _.InvokeExchanger(It.IsAny<LocalEndpoint>(), It.IsAny<Package>()));
            _endpointsInvoker.VerifyNoOtherCalls();
            _peer.Verify(_ => _.Send(It.IsAny<Package>()), Once);
            _courier.VerifyNoOtherCalls();
        }

        [Test]
        public void Handle_Response_Handle()
        {
            // Arrange
            _receivingMiddlewaresRunner.Setup(_ => _.Process(It.IsAny<Package>()))
                .Callback<Package>(package => package.IsResponse = true);

            // Act
            _controller.Handle(APackage(_peer.Object, ANetDataReader(), ReliableOrdered));

            // Assert
            _sendingMiddlewaresRunner.VerifyNoOtherCalls();
            _receivingMiddlewaresRunner.Verify(_ => _.Process(It.IsAny<Package>()), Once);
            _responsePackageMonitor.Verify(_ => _.Pulse(It.IsAny<Package>()), Once);
            _endpointsInvoker.VerifyNoOtherCalls();
            _peer.Verify(_ => _.Send(It.IsAny<Package>()), Never);
        }

        [Test]
        public void Handle_UnknownEndpoint_Throw()
        {
            // Arrange
            _receivingMiddlewaresRunner.Setup(_ => _.Process(It.IsAny<Package>()))
                .Callback(delegate(Package package)
                {
                    package.Route = new Route("another/test/route");
                    package.IsResponse = false;
                });
            _endpointsStorage.LocalEndpoints.Add(AReceiver());

            // Act
            Action act = () => _controller.Handle(APackage(_peer.Object, ANetDataReader(), ReliableOrdered));

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage("Package from peer 0 pointed to a non-existent endpoint. Route: another/test/route");
        }

        [Test]
        public void Handle_WrongReliability_Throw()
        {
            // Arrange
            _receivingMiddlewaresRunner.Setup(_ => _.Process(It.IsAny<Package>()))
                .Callback(delegate(Package package)
                {
                    package.Route = new Route("test/route");
                    package.IsResponse = false;
                });
            _endpointsStorage.LocalEndpoints.Add(AReceiver());

            // Act
            Action act = () => _controller.Handle(APackage(_peer.Object, ANetDataReader(), Unreliable));

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage("Package from 0 came with the wrong type of reliability");
        }

        [Test]
        public void Handle_InvocationException_Handle()
        {
            // Arrange
            ReceivingMiddlewaresRunnerProcessesPackage();
            _endpointsStorage.LocalEndpoints.Add(AnExchanger());
            _endpointsInvoker.Setup(_ => _.InvokeExchanger(It.IsAny<LocalEndpoint>(), It.IsAny<Package>()))
                .Returns(new Package
                {
                    NonSendingFields = { ["InvocationException"] = new ArithmeticException("endpoint run failed") }
                });

            var responseCapture = new List<Package>();
            _peer.Setup(_ => _.Send(Capture.In(responseCapture)));

            // Act
            _controller.Handle(APackage(_peer.Object, ANetDataReader(), ReliableOrdered));

            // Assert
            responseCapture[0].Error.Should().BeOfType<EndpointRunFailedView>();
            var errorView = (responseCapture[0].Error as EndpointRunFailedView)!;
            errorView.Message.Should().Be("endpoint run failed");
            errorView.Type.Should().Be(typeof(ArithmeticException));
        }

        private void ReceivingMiddlewaresRunnerProcessesPackage()
        {
            _receivingMiddlewaresRunner.Setup(_ => _.Process(It.IsAny<Package>()))
                .Callback(delegate(Package package)
                {
                    package.Route = new Route("test/route");
                    package.IsResponse = false;
                    package.ExchangeId = Guid.NewGuid();
                });
        }

        private static LocalEndpoint ALocalEndpoint(EndpointType endpointType)
        {
            return new LocalEndpoint(
                new Endpoint(
                    new Route("test/route"),
                    endpointType,
                    ReliableOrdered,
                    requestSchemaPatch: new PackageSchema(),
                    responseSchemaPatch: new PackageSchema()),
                action: new Func<Package>(() => new Package()));
        }

        private static LocalEndpoint AReceiver() => ALocalEndpoint(EndpointType.Receiver);

        private static LocalEndpoint AnExchanger() => ALocalEndpoint(EndpointType.Exchanger);

        private static LocalEndpoint AnInitializer => ALocalEndpoint(EndpointType.Initializer);

        private static NetDataReader ANetDataReader()
        {
            var netDataWriter = new NetDataWriter();
            netDataWriter.Put(UTF8.GetBytes("some-json-package"));
            return new NetDataReader(netDataWriter);
        }

        private static Package APackage(INetPeer peer, NetDataReader dataReader, Reliability reliability)
        {
            return new Package
            {
                Body = new NetworkReceiveBody { Peer = peer, DataReader = dataReader, Reliability = reliability }
            };
        }
    }
}
