using FluentAssertions;
using Kolyhalov.FatNetLib.Core;
using Kolyhalov.FatNetLib.Core.Modules.Encryption;
using Moq;
using NUnit.Framework;
using static Moq.Times;

namespace Kolyhalov.FatNetLib.Tests.Modules.Encryption;

public class ClientEncryptionServiceTests
{
    private ClientEncryptionService _service = null!;
    private Mock<IPeerRegistry> _encryptionRegistry = null!;
    private Mock<IPeerRegistry> _decryptionRegistry = null!;
    private Mock<IClient> _client = null!;
    private IDependencyContext _context = null!;

    [SetUp]
    public void SetUp()
    {
        _encryptionRegistry = new Mock<IPeerRegistry>();
        _decryptionRegistry = new Mock<IPeerRegistry>();
        _service = new ClientEncryptionService(_encryptionRegistry.Object, _decryptionRegistry.Object);
        _client = new Mock<IClient>();
        _context = new DependencyContext();
        _context.Put(_ => _client.Object);
    }

    [Test]
    public void ExchangePublicKeys_CorrectCase_RegisterPeerWithSharedSecret()
    {
        // Arrange
        var serverAlgorithm = new EcdhAlgorithm();

        byte[] actualSharedSecret = null!;
        _encryptionRegistry.Setup(_ => _.RegisterPeer(0, It.IsAny<byte[]>()))
            .Callback<int, byte[]>((_, key) => actualSharedSecret = key);

        // Act
        byte[] clientPublicKey = _service.ExchangePublicKeys(serverAlgorithm.MyPublicKey, serverPeerId: 0);

        // Assert
        _encryptionRegistry.Verify(_ => _.RegisterPeer(0, actualSharedSecret), Once);
        _encryptionRegistry.VerifyNoOtherCalls();
        _decryptionRegistry.Verify(_ => _.RegisterPeer(0, actualSharedSecret), Once);
        _decryptionRegistry.VerifyNoOtherCalls();
        byte[] expectedSharedSecret = serverAlgorithm.CalculateSharedSecret(clientPublicKey);
        actualSharedSecret.Should().BeEquivalentTo(expectedSharedSecret);
    }
}
