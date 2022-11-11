using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Subscribers;
using Kolyhalov.FatNetLib.Core.Wrappers;
using Moq;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Tests.Subscribers;

public class ServerPeerConnectedEventSubscriberTests
{
    private ServerPeerConnectedEventSubscriber _subscriber = null!;
    private Mock<IList<INetPeer>> _netPeers = null!;

    [SetUp]
    public void SetUp()
    {
        _netPeers = new Mock<IList<INetPeer>>();
        _subscriber = new ServerPeerConnectedEventSubscriber(_netPeers.Object);
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
