using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Runners;
using Kolyhalov.FatNetLib.Core.Subscribers.Client;
using Kolyhalov.FatNetLib.Core.Wrappers;
using Moq;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Core.Tests.Subscribers.Client
{
    public class ClientPeerConnectedEventControllerTests
    {
        private ClientPeerConnectedEventController _controller = null!;
        private Mock<IList<INetPeer>> _peers = null!;

        [SetUp]
        public void SetUp()
        {
            _peers = new Mock<IList<INetPeer>>();
            _controller = new ClientPeerConnectedEventController(
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
            _controller.Handle(new Package { Body = peer.Object });

            // Assert
            _peers.Verify(_ => _.Add(
                It.Is<INetPeer>(x => x == peer.Object)));
        }
    }
}
