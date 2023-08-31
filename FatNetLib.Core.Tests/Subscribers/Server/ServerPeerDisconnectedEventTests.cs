using System;
using System.Collections.Generic;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Subscribers.Server;
using Kolyhalov.FatNetLib.Core.Wrappers;
using LiteNetLib;
using Moq;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Core.Tests.Subscribers.Server
{
    public class ServerPeerDisconnectedEventTests
    {
        private ServerPeerDisconnectedEventController _controller = null!;
        private IList<INetPeer> _peers = null!;
        private IEndpointsStorage _endpointsStorage = null!;
        private Mock<INetPeer> _peer = null!;

        [SetUp]
        public void SetUp()
        {
            _peers = new List<INetPeer>();
            _endpointsStorage = new EndpointsStorage();
            _peer = new Mock<INetPeer>();
            _peer.Setup(_ => _.Id).Returns(Guid.NewGuid());
            _controller = new ServerPeerDisconnectedEventController(_peers, _endpointsStorage);
        }

        [Test]
        public void Handle_Peer_RemoveFromPeers()
        {
            // Arrange
            _peers.Add(_peer.Object);
            _endpointsStorage.RemoteEndpoints[_peer.Object.Id] = new List<Endpoint>();

            // Act
            _controller.Handle(APackage(_peer.Object, info: default));

            // Assert
            _peers.Should().NotContain(_peer.Object);
            _endpointsStorage.RemoteEndpoints.Should().NotContainKey(_peer.Object.Id);
        }

        [Test]
        public void Handle_NotExistingPeer_Pass()
        {
            // Act
            _controller.Handle(APackage(_peer.Object, info: default));

            // Assert
            _peers.Should().NotContain(_peer.Object);
            _endpointsStorage.RemoteEndpoints.Should().NotContainKey(_peer.Object.Id);
        }

        private static Package APackage(INetPeer peer, DisconnectInfo info)
        {
            return new Package { Body = new PeerDisconnectedBody { DisconnectInfo = info, Peer = peer } };
        }
    }
}
