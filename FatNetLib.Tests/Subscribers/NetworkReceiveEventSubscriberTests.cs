using System;
using System.Collections.Generic;
using FluentAssertions;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Microtypes;
using Kolyhalov.FatNetLib.Middlewares;
using Kolyhalov.FatNetLib.Monitors;
using Kolyhalov.FatNetLib.Utils;
using Kolyhalov.FatNetLib.Wrappers;
using LiteNetLib;
using LiteNetLib.Utils;
using Moq;
using NUnit.Framework;
using static System.Text.Encoding;
using static Moq.Times;

namespace Kolyhalov.FatNetLib.Subscribers;

public class NetworkReceiveEventSubscriberTests
{
    private readonly Mock<DependencyContext> _context = new();
    private readonly PackageSchema _defaultSchema = new();
    private INetworkReceiveEventSubscriber _subscriber = null!;
    private Mock<IResponsePackageMonitor> _responsePackageMonitor = null!;
    private Mock<INetPeer> _netPeer = null!;
    private Mock<IMiddlewaresRunner> _sendingMiddlewaresRunner = null!;
    private Mock<IMiddlewaresRunner> _receivingMiddlewaresRunner = null!;
    private Mock<IEndpointsInvoker> _endpointsInvoker = null!;
    private IEndpointsStorage _endpointsStorage = null!;

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

        _subscriber = new NetworkReceiveEventSubscriber(
            _responsePackageMonitor.Object,
            _receivingMiddlewaresRunner.Object,
            _defaultSchema,
            _context.Object,
            _endpointsStorage,
            _endpointsInvoker.Object,
            _sendingMiddlewaresRunner.Object,
            connectedPeers);
    }

    [Test]
    public void Handle_Receiver_Handle()
    {
        // Arrange
        _receivingMiddlewaresRunner.Setup(_ => _.Process(It.IsAny<Package>()))
            .Callback(delegate(Package package) { package.Route = new Route("test/route"); });
        _endpointsStorage.LocalEndpoints.Add(AReceiver());

        // Act
        _subscriber.Handle(_netPeer.Object, ANetDataReader(), DeliveryMethod.ReliableOrdered);

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
        _subscriber.Handle(_netPeer.Object, ANetDataReader(), DeliveryMethod.ReliableOrdered);

        // Assert
        _receivingMiddlewaresRunner.Verify(_ => _.Process(It.IsAny<Package>()), Once);
        _receivingMiddlewaresRunner.Verify(_ => _.Process(It.IsAny<Package>()), Once);
        _responsePackageMonitor.VerifyNoOtherCalls();
        _endpointsInvoker.Verify(_ => _.InvokeExchanger(It.IsAny<LocalEndpoint>(), It.IsAny<Package>()));
        _endpointsInvoker.VerifyNoOtherCalls();
        _netPeer.Verify(_ => _.Send(It.IsAny<Package>()), Once);
    }

    [Test]
    public void Handle_ExchangerResponse_Handle()
    {
        // Arrange
        _receivingMiddlewaresRunner.Setup(_ => _.Process(It.IsAny<Package>()))
            .Callback<Package>(package => package.IsResponse = true);

        // Act
        _subscriber.Handle(_netPeer.Object, ANetDataReader(), DeliveryMethod.ReliableOrdered);

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
        Action act = () => _subscriber.Handle(_netPeer.Object, ANetDataReader(), DeliveryMethod.ReliableOrdered);

        // Assert
        act.Should().Throw<FatNetLibException>()
            .WithMessage("Package from 0 pointed to a non-existent endpoint. Route: another/test/route");
    }

    [Test]
    public void Handle_WrongDeliveryMethod_Throw()
    {
        // Arrange
        _receivingMiddlewaresRunner.Setup(_ => _.Process(It.IsAny<Package>()))
            .Callback(delegate(Package package) { package.Route = new Route("test/route"); });
        _endpointsStorage.LocalEndpoints.Add(AReceiver());

        // Act
        Action act = () => _subscriber.Handle(_netPeer.Object, ANetDataReader(), DeliveryMethod.Unreliable);

        // Assert
        act.Should().Throw<FatNetLibException>()
            .WithMessage("Package from 0 came with the wrong type of delivery");
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
        _subscriber.Handle(_netPeer.Object, ANetDataReader(), DeliveryMethod.ReliableOrdered);

        // Assert
        receivedPackage.Serialized.Should().BeEquivalentToUtf8("some-json-package");
        receivedPackage.Schema.Should().BeSameAs(_defaultSchema);
        receivedPackage.Context.Should().Be(_context.Object);
        receivedPackage.FromPeerId.Should().Be(PeerId);
        receivedPackage.DeliveryMethod.Should().Be(DeliveryMethod.ReliableOrdered);
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
        _subscriber.Handle(_netPeer.Object, ANetDataReader(), DeliveryMethod.ReliableOrdered);

        // Assert
        packageToSend.IsResponse.Should().BeTrue();
    }

    private static LocalEndpoint ALocalEndpoint(EndpointType endpointType)
    {
        return new LocalEndpoint(
            new Endpoint(
                new Route("test/route"),
                endpointType,
                DeliveryMethod.ReliableOrdered,
                isInitial: false,
                requestSchemaPatch: new PackageSchema(),
                responseSchemaPatch: new PackageSchema()),
            methodDelegate: () => new Package());
    }

    private static LocalEndpoint AReceiver() => ALocalEndpoint(EndpointType.Receiver);

    private static LocalEndpoint AnExchanger() => ALocalEndpoint(EndpointType.Exchanger);

    private static NetDataReader ANetDataReader()
    {
        var netDataWriter = new NetDataWriter();
        netDataWriter.Put(UTF8.GetBytes("some-json-package"));
        return new NetDataReader(netDataWriter);
    }
}
