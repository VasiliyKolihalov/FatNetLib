using System;
using System.Threading.Tasks;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Components;
using Kolyhalov.FatNetLib.Core.Components.Server;
using Kolyhalov.FatNetLib.Core.Couriers;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Wrappers;
using Moq;
using NUnit.Framework;
using static Moq.Times;

namespace Kolyhalov.FatNetLib.Core.Tests.Components;

public class ServerEncryptionServiceTests
{
    private readonly Mock<INetPeer> _peer = new();
    private ServerEncryptionService _service = null!;
    private Mock<IEncryptionPeerRegistry> _encryptionRegistry = null!;
    private Mock<IEncryptionPeerRegistry> _decryptionRegistry = null!;
    private Mock<ICourier> _courier = null!;

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
        _service = new ServerEncryptionService(_encryptionRegistry.Object, _decryptionRegistry.Object);
        _courier = new Mock<ICourier>();
    }

    [Test]
    public async Task ExchangePublicKeysAsync_CorrectCase_RegisterPeerWithSharedSecret()
    {
        // Arrange
        var clientAlgorithm = new EcdhAlgorithm();

        byte[] expectedSharedSecret = null!;
        _courier.Setup(_ => _.SendAsync(It.IsAny<Package>()))
            .Callback<Package>(package =>
                expectedSharedSecret = clientAlgorithm.CalculateSharedSecret(package.GetBodyAs<byte[]>()))
            .Returns(Task.Run(() =>
            {
                var result = (Package?)new Package
                {
                    Body = clientAlgorithm.MyPublicKey,
                    Sender = _peer.Object
                };
                return result;
            }));

        byte[] actualSharedSecret = null!;
        _encryptionRegistry.Setup(_ => _.RegisterPeer(_peer.Object, It.IsAny<byte[]>()))
            .Callback<INetPeer, byte[]>((_, key) => actualSharedSecret = key);

        // Act
        await _service.ExchangePublicKeysAsync(_peer.Object, _courier.Object);

        // Assert
        _courier.Verify(_ => _.SendAsync(It.IsAny<Package>()), Once);
        _courier.VerifyNoOtherCalls();
        _encryptionRegistry.Verify(_ => _.RegisterPeer(_peer.Object, actualSharedSecret), Once);
        _encryptionRegistry.VerifyNoOtherCalls();
        _decryptionRegistry.Verify(_ => _.RegisterPeer(_peer.Object, actualSharedSecret), Once);
        _decryptionRegistry.VerifyNoOtherCalls();
        actualSharedSecret.Should().BeEquivalentTo(expectedSharedSecret);
    }
}
