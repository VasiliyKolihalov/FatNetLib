using System.Collections.Generic;
using FluentAssertions;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Wrappers;
using Moq;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Subscribers;

public class ServerPeerDisconnectedEventTests
{
    private ServerPeerDisconnectedEventSubscriber _subscriber = null!;
    private IList<INetPeer> _peers = null!;
    private IEndpointsStorage _endpointsStorage = null!;
    private Mock<INetPeer> _peer = null!;

    [SetUp]
    public void SetUp()
    {
        _peers = new List<INetPeer>();
        _endpointsStorage = new EndpointsStorage();
        _peer = new Mock<INetPeer>();
        _peer.Setup(_ => _.Id).Returns(42);
        _subscriber = new ServerPeerDisconnectedEventSubscriber(_peers, _endpointsStorage);
    }

    [Test]
    public void Handle_Peer_RemoveFromPeers()
    {
        // Arrange
        _peers.Add(_peer.Object);
        _endpointsStorage.RemoteEndpoints[_peer.Object.Id] = new List<Endpoint>();

        // Act
        _subscriber.Handle(_peer.Object, info: default);

        // Assert
        _peers.Should().NotContain(_peer.Object);
        _endpointsStorage.RemoteEndpoints.Should().NotContainKey(_peer.Object.Id);
    }

    [Test]
    public void Handle_NotExistingPeer_Pass()
    {
        // Act
        _subscriber.Handle(_peer.Object, info: default);

        // Assert
        _peers.Should().NotContain(_peer.Object);
        _endpointsStorage.RemoteEndpoints.Should().NotContainKey(_peer.Object.Id);
    }
}