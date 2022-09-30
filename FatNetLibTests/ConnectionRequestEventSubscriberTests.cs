using AutoFixture.NUnit3;
using Kolyhalov.FatNetLib;
using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.LiteNetLibWrappers;
using Kolyhalov.FatNetLib.Microtypes;
using Kolyhalov.FatNetLib.Subscribers;
using LiteNetLib.Utils;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using static Moq.Times;

namespace FatNetLibTests;

public class ConnectionRequestEventSubscriberTests
{
    private ConnectionRequestEventSubscriber _subscriber = null!;
    private Mock<INetManager> _netManager = null!;
    private Mock<IProtocolVersionProvider> _protocolVersionProvider = null!;
    private Mock<IConnectionRequest> _connectionRequest = null!;

    private readonly ServerConfiguration _configuration = new(new Port(8080),
        maxPeers: new Count(5),
        framerate: null,
        exchangeTimeout: null);

    private readonly Mock<ILogger> _logger = new();

    [SetUp]
    public void SetUp()
    {
        _netManager = new Mock<INetManager>();
        _protocolVersionProvider = new Mock<IProtocolVersionProvider>();

        _connectionRequest = new Mock<IConnectionRequest>();
    }

    [Test, AutoData]
    public void Handle_GoodConnection_AcceptRequest()
    {
        // Arrange
        _protocolVersionProvider.Setup(_ => _.Get())
            .Returns("some-version");
        NetDataReader netDataReader = ANetDataReader(content: "some-version");
        _connectionRequest.Setup(_ => _.Data)
            .Returns(netDataReader);
        InitializeConnectionRequestEventSubscriber();

        // Act
        _subscriber.Handle(_connectionRequest.Object);

        // Assert
        _connectionRequest.Verify(_ => _.Data);
        _connectionRequest.Verify(_ => _.Accept(), Once);
        _connectionRequest.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public void Handle_MaxPeersReached_RejectRequest()
    {
        // Arrange
        _netManager.Setup(_ => _.ConnectedPeersCount)
            .Returns(5);
        _protocolVersionProvider.Setup(_ => _.Get())
            .Returns("some-version");
        NetDataReader netDataReader = ANetDataReader(content: "some-version");
        _connectionRequest.Setup(_ => _.Data)
            .Returns(netDataReader);
        InitializeConnectionRequestEventSubscriber();

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
        _protocolVersionProvider.Setup(_ => _.Get())
            .Returns("some-version");
        NetDataReader netDataReader = ANetDataReader(content: "another-version");
        _connectionRequest.Setup(_ => _.Data)
            .Returns(netDataReader);
        InitializeConnectionRequestEventSubscriber();

        // Act
        _subscriber.Handle(_connectionRequest.Object);

        // Assert
        _connectionRequest.Verify(_ => _.Data);
        _connectionRequest.Verify(_ => _.Reject(), Once);
        _connectionRequest.VerifyNoOtherCalls();
        _logger.VerifyLogWarning("Connection rejected: Protocol version mismatch", Once);
    }

    private void InitializeConnectionRequestEventSubscriber()
    {
        _subscriber = new ConnectionRequestEventSubscriber(_configuration,
            _netManager.Object,
            _protocolVersionProvider.Object,
            _logger.Object);
    }

    private static NetDataReader ANetDataReader(string content)
    {
        var netDataWriter = new NetDataWriter();
        netDataWriter.Put(content);
        return new NetDataReader(netDataWriter);
    }
}