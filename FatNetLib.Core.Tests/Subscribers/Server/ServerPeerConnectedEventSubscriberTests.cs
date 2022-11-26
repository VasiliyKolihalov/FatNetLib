using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Subscribers.Server;
using Kolyhalov.FatNetLib.Core.Wrappers;
using Moq;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Core.Tests.Subscribers.Server
{
    public class ServerPeerConnectedEventSubscriberTests
    {
        private ServerPeerConnectedEventSubscriber _subscriber = null!;
        private Mock<IList<INetPeer>> _peers = null!;

        [SetUp]
        public void SetUp()
        {
            _peers = new Mock<IList<INetPeer>>();
            _subscriber = new ServerPeerConnectedEventSubscriber(_peers.Object);
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
