using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Modules.Encryption;
using Moq;
using NUnit.Framework;
using static Moq.Times;

namespace Kolyhalov.FatNetLib.Core.Tests.Modules.Encryption
{
    public class ServerEncryptionServiceTests
    {
        private ServerEncryptionService _service = null!;
        private Mock<IPeerRegistry> _encryptionRegistry = null!;
        private Mock<IPeerRegistry> _decryptionRegistry = null!;
        private Mock<IClient> _client = null!;

        [SetUp]
        public void SetUp()
        {
            _encryptionRegistry = new Mock<IPeerRegistry>();
            _decryptionRegistry = new Mock<IPeerRegistry>();
            _service = new ServerEncryptionService(_encryptionRegistry.Object, _decryptionRegistry.Object);
            _client = new Mock<IClient>();
        }

        [Test]
        public void ExchangePublicKeys_CorrectCase_RegisterPeerWithSharedSecret()
        {
            // Arrange
            var clientAlgorithm = new EcdhAlgorithm();

            byte[] expectedSharedSecret = null!;
            _client.Setup(_ => _.SendPackage(It.IsAny<Package>()))
                .Callback<Package>(package =>
                    expectedSharedSecret = clientAlgorithm.CalculateSharedSecret(package.GetBodyAs<byte[]>()))
                .Returns(new Package
                {
                    Body = clientAlgorithm.MyPublicKey,
                    FromPeerId = 0
                });

            byte[] actualSharedSecret = null!;
            _encryptionRegistry.Setup(_ => _.RegisterPeer(0, It.IsAny<byte[]>()))
                .Callback<int, byte[]>((_, key) => actualSharedSecret = key);

            // Act
            _service.ExchangePublicKeys(clientPeerId: 0, _client.Object);

            // Assert
            _client.Verify(_ => _.SendPackage(It.IsAny<Package>()), Once);
            _client.VerifyNoOtherCalls();
            _encryptionRegistry.Verify(_ => _.RegisterPeer(0, actualSharedSecret), Once);
            _encryptionRegistry.VerifyNoOtherCalls();
            _decryptionRegistry.Verify(_ => _.RegisterPeer(0, actualSharedSecret), Once);
            _decryptionRegistry.VerifyNoOtherCalls();
            actualSharedSecret.Should().BeEquivalentTo(expectedSharedSecret);
        }
    }
}
