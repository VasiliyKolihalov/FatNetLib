using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Subscribers.Server;
using Kolyhalov.FatNetLib.Core.Wrappers;
using Moq;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Core.Tests.Subscribers.Server
{
    public class ServerPeerConnectedEventControllerTests
    {
        private ServerPeerConnectedEventController _controller = null!;
        private Mock<IList<INetPeer>> _peers = null!;

        [SetUp]
        public void SetUp()
        {
            _peers = new Mock<IList<INetPeer>>();
            _controller = new ServerPeerConnectedEventController(_peers.Object);
        }

        [Test]
        public void Handle_SomeEvent_AddNewPeer()
        {
            // Arrange
            var peer = new Mock<INetPeer>();

            // Act
            _controller.Handle(new Package { Body = peer.Object });

            // Assert
            _peers.Verify(_ => _.Add(
                It.Is<INetPeer>(x => x == peer.Object)));
        }
    }
}
