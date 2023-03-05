using System.Collections.Generic;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Subscribers.Client;
using Kolyhalov.FatNetLib.Core.Wrappers;
using LiteNetLib;
using Moq;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Core.Tests.Subscribers.Client
{
    public class ClientPeerDisconnectedEventControllerTests
    {
        private ClientPeerDisconnectedEventController _controller = null!;
        private IList<INetPeer> _peers = null!;
        private INetPeer _peer = null!;

        [SetUp]
        public void SetUp()
        {
            _peers = new List<INetPeer>();
            _peer = new Mock<INetPeer>().Object;
            _controller = new ClientPeerDisconnectedEventController(_peers);
        }

        [Test]
        public void Handle_Peer_RemoveFromPeers()
        {
            // Arrange
            _peers.Add(_peer);

            // Act
            _controller.Handle(APackage(_peer, info: default));

            // Assert
            _peers.Should().NotContain(_peer);
        }

        [Test]
        public void Handle_NotExistingPeer_Pass()
        {
            // Act
            _controller.Handle(APackage(_peer, info: default));

            // Assert
            _peers.Should().NotContain(_peer);
        }

        private static Package APackage(INetPeer peer, DisconnectInfo info)
        {
            return new Package { Body = new PeerDisconnectedBody { DisconnectInfo = info, Peer = peer } };
        }
    }
}
