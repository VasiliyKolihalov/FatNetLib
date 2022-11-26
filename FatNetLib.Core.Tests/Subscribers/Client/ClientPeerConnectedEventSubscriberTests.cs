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
        private Mock<IList<INetPeer>> _peers = null!;

        [SetUp]
        public void SetUp()
        {
            _peers = new Mock<IList<INetPeer>>();
            _subscriber = new ClientPeerConnectedEventSubscriber(
                _peers.Object,
                new Mock<IInitializersRunner>().Object,
                new Mock<ILogger>().Object);
        }

        [Test]
        public void Handle_SomeEvent_AddNewPeer()
        {
            // Arrange
            var peer = new Mock<INetPeer>();

            // Act
            _subscriber.Handle(peer.Object);

            // Assert
            _peers.Verify(_ => _.Add(
                It.Is<INetPeer>(x => x == peer.Object)));
        }
    }
}
