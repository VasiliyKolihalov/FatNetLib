using AutoFixture.NUnit3;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Providers;
using Kolyhalov.FatNetLib.Core.Subscribers.Server;
using Kolyhalov.FatNetLib.Core.Wrappers;
using LiteNetLib.Utils;
using Moq;
using NUnit.Framework;
using static Moq.Times;

namespace Kolyhalov.FatNetLib.Core.Tests.Subscribers.Server
{
    public class ServerConnectionRequestEventSubscriberTests
    {
        private readonly Mock<ILogger> _logger = new Mock<ILogger>();
        private ServerConnectionRequestEventSubscriber _subscriber = null!;
        private Mock<INetManager> _netManager = null!;
        private Mock<IConnectionRequest> _connectionRequest = null!;

        [SetUp]
        public void SetUp()
        {
            _netManager = new Mock<INetManager>();
            _connectionRequest = new Mock<IConnectionRequest>();

            var protocolVersionProvider = new Mock<IProtocolVersionProvider>();
            protocolVersionProvider.Setup(_ => _.Get())
                .Returns("some-version");

            _subscriber = new ServerConnectionRequestEventSubscriber(
                maxPeers: new Count(5),
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
            _connectionRequest.Verify(_ => _.Reject(), Never);
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
            _connectionRequest.Verify(_ => _.Accept(), Never);
            _logger.Verify(_ => _.Warn("Connection rejected: Max peers exceeded"), Once);
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
            _connectionRequest.Verify(_ => _.Accept(), Never);
            _logger.Verify(_ => _.Warn("Connection rejected: Protocol version mismatch"), Once);
        }

        private static NetDataReader ANetDataReader(string content)
        {
            var netDataWriter = new NetDataWriter();
            netDataWriter.Put(content);
            return new NetDataReader(netDataWriter);
        }
    }
}
