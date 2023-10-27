using System;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Components;
using Kolyhalov.FatNetLib.Core.Components.Client;
using Kolyhalov.FatNetLib.Core.Couriers;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Wrappers;
using Moq;
using NUnit.Framework;
using static Moq.Times;

namespace Kolyhalov.FatNetLib.Core.Tests.Components;

public class ClientEncryptionServiceTests
{
    private readonly Mock<INetPeer> _peer = new Mock<INetPeer>();
    private ClientEncryptionService _service = null!;
    private Mock<IEncryptionPeerRegistry> _encryptionRegistry = null!;
    private Mock<IEncryptionPeerRegistry> _decryptionRegistry = null!;
    private Mock<ICourier> _courier = null!;
    private IDependencyContext _context = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _peer.Setup(_ => _.Id).Returns(Guid.NewGuid());
    }

    [SetUp]
    public void SetUp()
    {
        _encryptionRegistry = new Mock<IEncryptionPeerRegistry>();
        _decryptionRegistry = new Mock<IEncryptionPeerRegistry>();
        _service = new ClientEncryptionService(_encryptionRegistry.Object, _decryptionRegistry.Object);
        _courier = new Mock<ICourier>();
        _context = new DependencyContext();
        _context.Put(_courier.Object);
    }

    [Test]
    public void ExchangePublicKeys_CorrectCase_RegisterPeerWithSharedSecret()
    {
        // Arrange
        var serverAlgorithm = new EcdhAlgorithm();

        byte[] actualSharedSecret = null!;
        _encryptionRegistry.Setup(_ => _.RegisterPeer(_peer.Object, It.IsAny<byte[]>()))
            .Callback<INetPeer, byte[]>((_, key) => actualSharedSecret = key);

        // Act
        byte[] clientPublicKey = _service.ExchangePublicKeys(serverAlgorithm.MyPublicKey, _peer.Object);

        // Assert
        _encryptionRegistry.Verify(_ => _.RegisterPeer(_peer.Object, actualSharedSecret), Once);
        _encryptionRegistry.VerifyNoOtherCalls();
        _decryptionRegistry.Verify(_ => _.RegisterPeer(_peer.Object, actualSharedSecret), Once);
        _decryptionRegistry.VerifyNoOtherCalls();
        byte[] expectedSharedSecret = serverAlgorithm.CalculateSharedSecret(clientPublicKey);
        actualSharedSecret.Should().BeEquivalentTo(expectedSharedSecret);
    }
}
