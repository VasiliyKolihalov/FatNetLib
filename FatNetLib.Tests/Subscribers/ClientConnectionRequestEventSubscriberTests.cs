using System;
using AutoFixture.NUnit3;
using FluentAssertions;
using Kolyhalov.FatNetLib.Wrappers;
using LiteNetLib.Utils;
using Moq;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Subscribers;

public class ClientConnectionRequestEventSubscriberTests
{
    private readonly ClientConnectionRequestEventSubscriber _subscriber = new();
    private Mock<IConnectionRequest> _connectionRequest = null!;

    [SetUp]
    public void SetUp()
    {
        _connectionRequest = new Mock<IConnectionRequest>();
    }

    [Test, AutoData]
    public void Handle_Connection_DoNothing()
    {
        // Arrange
        NetDataReader netDataReader = ANetDataReader(content: "some-version");
        _connectionRequest.Setup(_ => _.Data)
            .Returns(netDataReader);

        // Act
        Action act = () => _subscriber.Handle(_connectionRequest.Object);

        // Assert
        act.Should().NotThrow();
    }

    private static NetDataReader ANetDataReader(string content)
    {
        var netDataWriter = new NetDataWriter();
        netDataWriter.Put(content);
        return new NetDataReader(netDataWriter);
    }
}
