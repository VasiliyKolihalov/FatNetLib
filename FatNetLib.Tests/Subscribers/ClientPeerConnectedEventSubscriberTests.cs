using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Initializers;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Subscribers;
using Kolyhalov.FatNetLib.Core.Wrappers;
using Moq;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Tests.Subscribers;

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
            new Mock<IInitialEndpointsRunner>().Object,
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
