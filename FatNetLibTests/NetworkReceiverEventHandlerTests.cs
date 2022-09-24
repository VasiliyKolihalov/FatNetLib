﻿using AutoFixture;
using AutoFixture.NUnit3;
using Kolyhalov.FatNetLib;
using Kolyhalov.FatNetLib.Middlewares;
using Kolyhalov.FatNetLib.NetPeers;
using Kolyhalov.FatNetLib.ResponsePackageMonitors;
using LiteNetLib;
using LiteNetLib.Utils;
using Moq;
using NUnit.Framework;
using static Moq.Times;

namespace FatNetLibTests;

public class NetworkReceiverEventHandlerTests
{
    private INetworkReceiveEventHandler _networkReceiveEventHandler = null!;
    private Mock<IResponsePackageMonitor> _responsePackageMonitor = null!;
    private Mock<IPackageHandler> _packageHandler = null!;
    private Mock<INetPeer> _netPeer = null!;
    private Mock<IMiddlewaresRunner> _middlewaresRunner = null!;

    private int NetPeerId => _netPeer.Object.Id;

    [SetUp]
    public void SetUp()
    {
        _responsePackageMonitor = new Mock<IResponsePackageMonitor>();
        _packageHandler = new Mock<IPackageHandler>();
        _middlewaresRunner = new Mock<IMiddlewaresRunner>();
        _networkReceiveEventHandler =
            new NetworkReceiveEventHandler(_packageHandler.Object,
                _responsePackageMonitor.Object,
                _middlewaresRunner.Object,
                new PackageSchema());
    }

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _netPeer = new Mock<INetPeer>();
        _netPeer.Setup(netPeer => netPeer.Id)
            .Returns(new Fixture().Create<int>());
    }

    [Test, AutoData]
    public void Handle_RequestPackage_InvokePackageHandler(DeliveryMethod deliveryMethod)
    {
        // Arrange
        _middlewaresRunner.Setup(_ => _.Process(It.IsAny<Package>()))
            .Callback<Package>(package =>
            {
                package.Route = "some-route";
                package.IsResponse = false;
            });
        NetDataReader netDataReader = ANetDataReader();

        // Act
        _networkReceiveEventHandler.Handle(_netPeer.Object, netDataReader, deliveryMethod);

        // Assert
        _packageHandler.Verify(_ => _.Handle(It.IsAny<Package>(), NetPeerId, deliveryMethod), Once);
        _packageHandler.VerifyNoOtherCalls();
        _responsePackageMonitor.VerifyNoOtherCalls();
    }

    [Test]
    public void Handle_ResponsePackage_PulseResponsePackageMonitor()
    {
        // Arrange
        _middlewaresRunner.Setup(_ => _.Process(It.IsAny<Package>()))
            .Callback<Package>(package =>
            {
                package.Route = "some-route";
                package.IsResponse = true;
            });
        NetDataReader netDataReader = ANetDataReader();

        // Act
        _networkReceiveEventHandler.Handle(null!, netDataReader, It.IsAny<DeliveryMethod>());

        // Assert
        _packageHandler.VerifyNoOtherCalls();
        _responsePackageMonitor.Verify(_ => _.Pulse(It.IsAny<Package>()), Once);
        _responsePackageMonitor.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public void Handle_ConnectionPackageRoute_Return(DeliveryMethod deliveryMethod)
    {
        // Arrange
        _middlewaresRunner.Setup(_ => _.Process(It.IsAny<Package>()))
            .Callback<Package>(package =>
            {
                package.Route = "connection";
                package.IsResponse = true;
            });
        NetDataReader netDataReader = ANetDataReader();

        // Act
        _networkReceiveEventHandler.Handle(null!, netDataReader, deliveryMethod);

        // Assert
        _packageHandler.VerifyNoOtherCalls();
        _responsePackageMonitor.VerifyNoOtherCalls();
    }

    private static NetDataReader ANetDataReader()
    {
        var netDataWriter = new NetDataWriter();
        netDataWriter.Put("some-json-package");
        return new NetDataReader(netDataWriter);
    }
}