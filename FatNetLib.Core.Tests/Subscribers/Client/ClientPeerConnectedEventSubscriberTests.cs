using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Runners;
using Kolyhalov.FatNetLib.Core.Subscribers.Client;
using Kolyhalov.FatNetLib.Core.Wrappers;
using Moq;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Core.Tests.Subscribers.Client
{
    public class ClientPeerConnectedEventSubscriberTests
    {
        private ClientPeerConnectedEventSubscriber _subscriber = null!;
        private Mock<IList<INetPeer>> _netPeers = null!;

        [SetUp]
        public void SetUp()
        {
            _netPeers = new Mock<IList<INetPeer>>();
            _subscriber = new ClientPeerConnectedEventSubscriber(
                _netPeers.Object,
                new Mock<IInitializersRunner>().Object,
                new Mock<ILogger>().Object);
        }

        [Test]
        public void Handle_SomeEvent_AddNewPeer()
        {
            // Arrange
            var netPeer = new Mock<INetPeer>();

            // Act
            _subscriber.Handle(netPeer.Object);

            // Assert
            _netPeers.Verify(_ => _.Add(
                It.Is<INetPeer>(peer => peer == netPeer.Object)));
        }
    }
}
