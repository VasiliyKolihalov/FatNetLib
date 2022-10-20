using AutoFixture.NUnit3;
using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Microtypes;
using Kolyhalov.FatNetLib.Utils;
using Kolyhalov.FatNetLib.Wrappers;
using LiteNetLib.Utils;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using static Moq.Times;

namespace Kolyhalov.FatNetLib.Subscribers;

public class ServerConnectionRequestEventSubscriberTests
{
    private ServerConnectionRequestEventSubscriber _subscriber = null!;
    private Mock<INetManager> _netManager = null!;
    private Mock<IConnectionRequest> _connectionRequest = null!;

    private readonly Mock<ILogger> _logger = new();

    [SetUp]
    public void SetUp()
    {
        _netManager = new Mock<INetManager>();
        _connectionRequest = new Mock<IConnectionRequest>();

        var protocolVersionProvider = new Mock<IProtocolVersionProvider>();
        protocolVersionProvider.Setup(_ => _.Get())
            .Returns("some-version");

        var configuration = new ServerConfiguration(new Port(8080),
            maxPeers: new Count(5),
            framerate: null,
            exchangeTimeout: null);

        _subscriber = new ServerConnectionRequestEventSubscriber(configuration,
            _netManager.Object,
            protocolVersionProvider.Object,
            _logger.Object);
    }

    [Test, AutoData]
    public void Handle_GoodConnection_AcceptRequest()
    {
        // Arrange
        _connectionRequest.Setup(_ => _.Data)
            .Returns(ANetDataReader(content: "some-version"));

        // Act
        _subscriber.Handle(_connectionRequest.Object);

        // Assert
        _connectionRequest.Verify(_ => _.Accept(), Once);
        _connectionRequest.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public void Handle_MaxPeersReached_RejectRequest()
    {
        // Arrange
        _netManager.Setup(_ => _.ConnectedPeersCount)
            .Returns(5);
        _connectionRequest.Setup(_ => _.Data)
            .Returns(ANetDataReader(content: "some-version"));

        // Act
        _subscriber.Handle(_connectionRequest.Object);

        // Assert
        _connectionRequest.Verify(_ => _.Reject(), Once);
        _connectionRequest.VerifyNoOtherCalls();
        _logger.VerifyLogWarning("Connection rejected: Max peers exceeded", Once);
    }

    [Test, AutoData]
    public void Handle_ProtocolVersionMismatch_RejectRequest()
    {
        // Arrange
        _connectionRequest.Setup(_ => _.Data)
            .Returns(ANetDataReader(content: "another-version"));

        // Act
        _subscriber.Handle(_connectionRequest.Object);

        // Assert
        _connectionRequest.Verify(_ => _.Reject(), Once);
        _connectionRequest.VerifyNoOtherCalls();
        _logger.VerifyLogWarning("Connection rejected: Protocol version mismatch", Once);
    }

    private static NetDataReader ANetDataReader(string content)
    {
        var netDataWriter = new NetDataWriter();
        netDataWriter.Put(content);
        return new NetDataReader(netDataWriter);
    }
}