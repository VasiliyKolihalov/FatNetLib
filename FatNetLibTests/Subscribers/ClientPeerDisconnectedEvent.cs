﻿using System;
using System.Collections.Generic;
using FluentAssertions;
using Kolyhalov.FatNetLib.Wrappers;
using Moq;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Subscribers;

public class ClientPeerDisconnectedEventTests
{
    private ClientPeerDisconnectedEventSubscriber _subscriber = null!;
    private IList<INetPeer> _peers = null!;
    private INetPeer _peer = null!;

    [SetUp]
    public void SetUp()
    {
        _peers = new List<INetPeer>();
        _peer = new Mock<INetPeer>().Object;
        _subscriber = new ClientPeerDisconnectedEventSubscriber(_peers);
    }

    [Test]
    public void Handle_Peer_RemoveFromPeers()
    {
        // Arrange
        _peers.Add(_peer);

        // Act
        _subscriber.Handle(_peer, info: default);

        // Assert
        _peers.Should().NotContain(_peer);
    }

    [Test]
    public void Handle_NotExistingPeer_Pass()
    {
        // Act
        _subscriber.Handle(_peer, info: default);

        // Assert
        _peers.Should().NotContain(_peer);
    }
}