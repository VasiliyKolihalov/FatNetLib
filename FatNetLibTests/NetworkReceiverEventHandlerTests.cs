using System;
using AutoFixture;
using AutoFixture.NUnit3;
using FluentAssertions;
using Kolyhalov.FatNetLib;
using Kolyhalov.FatNetLib.NetPeers;
using Kolyhalov.FatNetLib.ResponsePackageMonitors;
using LiteNetLib;
using LiteNetLib.Utils;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using static Moq.Times;

namespace FatNetLibTests;

public class NetworkReceiverEventHandlerTests
{
    private INetworkReceiveEventHandler _networkReceiveEventHandler = null!;
    private Mock<IResponsePackageMonitor> _responsePackageMonitor = null!;
    private Mock<IPackageHandler> _packageHandler = null!;
    private Mock<INetPeer> _netPeer = null!;

    private int NetPeerId => _netPeer.Object.Id;

    [SetUp]
    public void SetUp()
    {
        _responsePackageMonitor = new Mock<IResponsePackageMonitor>();
        _packageHandler = new Mock<IPackageHandler>();
        _networkReceiveEventHandler =
            new NetworkReceiveEventHandler(_packageHandler.Object, _responsePackageMonitor.Object);
    }

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _netPeer = new Mock<INetPeer>();
        _netPeer.Setup(netPeer => netPeer.Id)
            .Returns(new Fixture().Create<int>());
    }

    [Test, AutoData]
    public void Handle_NotResponsePackage_InvokePackageHandler(DeliveryMethod deliveryMethod)
    {
        // Arrange
        var netDataWriter = new NetDataWriter();
        var package = new Package
        {
            Route = "some-route"
        };
        netDataWriter.Put(JsonConvert.SerializeObject(package));
        var netDataReader = new NetDataReader(netDataWriter);

        // Act
        _networkReceiveEventHandler.Handle(_netPeer.Object, netDataReader, deliveryMethod);

        // Assert
        _packageHandler.Verify(x => x.Handle(It.IsAny<Package>(), NetPeerId, deliveryMethod), Once);
        _packageHandler.VerifyNoOtherCalls();
        _responsePackageMonitor.VerifyNoOtherCalls();
    }

    [Test]
    public void Handle_ResponsePackage_PulseResponsePackageMonitor()
    {
        // Arrange
        var netDataWriter = new NetDataWriter();
        var package = new Package
        {
            Route = "some-route",
            IsResponse = true
        };
        netDataWriter.Put(JsonConvert.SerializeObject(package));
        var netDataReader = new NetDataReader(netDataWriter);

        // Act
        _networkReceiveEventHandler.Handle(null!, netDataReader, It.IsAny<DeliveryMethod>());

        // Assert
        _packageHandler.VerifyNoOtherCalls();
        _responsePackageMonitor.Verify(x => x.Pulse(It.IsAny<Package>()), Once);
        _responsePackageMonitor.VerifyNoOtherCalls();
    }

    [Test]
    public void Handle_NullPackage_Throw()
    {
        // Arrange
        var netDataWriter = new NetDataWriter();
        Package package = null!;
        netDataWriter.Put(JsonConvert.SerializeObject(package));
        var netDataReader = new NetDataReader(netDataWriter);

        // Act
        Action action = () => _networkReceiveEventHandler.Handle(null!, netDataReader, It.IsAny<DeliveryMethod>());

        // Assert
        action.Should()
            .Throw<FatNetLibException>()
            .WithMessage("Failed to deserialize package")
            .WithInnerException<FatNetLibException>()
            .WithMessage("Deserialized package is null");
    }
    
    [Test, AutoData]
    public void Handle_ConnectionPackageRoute_Return(DeliveryMethod deliveryMethod)
    {
        // Arrange
        var netDataWriter = new NetDataWriter();
        var package = new Package
        {
            Route = "connection"
        };
        netDataWriter.Put(JsonConvert.SerializeObject(package));
        var netDataReader = new NetDataReader(netDataWriter);

        // Act
        _networkReceiveEventHandler.Handle(null!, netDataReader, deliveryMethod);

        // Assert
        _packageHandler.VerifyNoOtherCalls();
        _responsePackageMonitor.VerifyNoOtherCalls();
    }
    
}