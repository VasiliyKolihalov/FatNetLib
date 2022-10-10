using AutoFixture;
using AutoFixture.NUnit3;
using FluentAssertions;
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
    private INetworkReceiveEventSubscriber _networkReceiveEventSubscriber = null!;
    private Mock<IResponsePackageMonitor> _responsePackageMonitor = null!;
    private Mock<IPackageHandler> _packageHandler = null!;
    private Mock<INetPeer> _netPeer = null!;
    private Mock<IMiddlewaresRunner> _middlewaresRunner = null!;
    private readonly Mock<DependencyContext> _context = new();
    private readonly PackageSchema _schema = new();

    private int PeerId => _netPeer.Object.Id;

    [SetUp]
    public void SetUp()
    {
        _responsePackageMonitor = new Mock<IResponsePackageMonitor>();
        _packageHandler = new Mock<IPackageHandler>();
        _middlewaresRunner = new Mock<IMiddlewaresRunner>();
        _networkReceiveEventSubscriber =
            new NetworkReceiveEventSubscriber(_packageHandler.Object,
                _responsePackageMonitor.Object,
                _middlewaresRunner.Object,
                _schema,
                _context.Object);
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
            .Callback<Package>(lambdaPackage =>
            {
                lambdaPackage.Route = new Route("some-route");
                lambdaPackage.IsResponse = false;
            });
        NetDataReader netDataReader = ANetDataReader();

        // Act
        _networkReceiveEventSubscriber.Handle(_netPeer.Object, netDataReader, deliveryMethod);

        // Assert
        _packageHandler.Verify(_ => _.Handle(It.IsAny<Package>()), Once);
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
                package.Route = new Route("some-route");
                package.IsResponse = true;
            });
        NetDataReader netDataReader = ANetDataReader();

        // Act
        _networkReceiveEventSubscriber.Handle(_netPeer.Object, netDataReader, It.IsAny<DeliveryMethod>());

        // Assert
        _packageHandler.VerifyNoOtherCalls();
        _responsePackageMonitor.Verify(_ => _.Pulse(It.IsAny<Package>()), Once);
        _responsePackageMonitor.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public void Handle_SomeEvent_ProvideValidPackage(DeliveryMethod deliveryMethod)
    {
        // Arrange
        Package capturedPackage = null!;
        _middlewaresRunner.Setup(_ => _.Process(It.IsAny<Package>()))
            .Callback<Package>(package => capturedPackage = package);

        // Act
        _networkReceiveEventSubscriber.Handle(_netPeer.Object, ANetDataReader(), deliveryMethod);

        // Assert
        capturedPackage.Serialized.Should().BeEquivalentToUtf8("some-json-package");
        capturedPackage.Context.Should().Be(_context.Object);
        capturedPackage.Schema.Should().BeSameAs(_schema);
        capturedPackage.FromPeerId.Should().Be(PeerId);
        capturedPackage.DeliveryMethod.Should().Be(deliveryMethod);
    }

    private static NetDataReader ANetDataReader()
    {
        var netDataWriter = new NetDataWriter();
        netDataWriter.Put(UTF8.GetBytes("some-json-package"));
        return new NetDataReader(netDataWriter);
    }
}