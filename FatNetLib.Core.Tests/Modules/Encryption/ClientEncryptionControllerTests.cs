using AutoFixture.NUnit3;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Modules.Encryption;
using Moq;
using NUnit.Framework;
using static Moq.Times;

namespace Kolyhalov.FatNetLib.Core.Tests.Modules.Encryption
{
    public class ClientEncryptionControllerTests
    {
        private ClientEncryptionController _controller = null!;
        private Mock<IClientEncryptionService> _service = null!;
        private Mock<IClient> _client = null!;
        private IDependencyContext _context = null!;

        [SetUp]
        public void SetUp()
        {
            _service = new Mock<IClientEncryptionService>();
            _controller = new ClientEncryptionController(_service.Object);
            _client = new Mock<IClient>();
            _context = new DependencyContext();
            _context.Put(_ => _client.Object);
        }

        [Test, AutoData]
        public void ExchangePublicKeys_CorrectCase_RegisterPeerWithSharedSecret(
            byte[] serverPublicKey,
            byte[] clientPublicKey)
        {
            // Arrange
            var serverPublicKeyPackage = new Package
            {
                FromPeerId = 0,
                Context = _context,
                Body = serverPublicKey
            };
            _service.Setup(_ => _.ExchangePublicKeys(It.IsAny<byte[]>(), It.IsAny<int>()))
                .Returns(clientPublicKey);

            // Act
            Package clientPublicKeyPackage = _controller.ExchangePublicKeys(serverPublicKeyPackage);

            // Assert
            _service.Verify(_ => _.ExchangePublicKeys(serverPublicKey, 0), Once);
            _service.VerifyNoOtherCalls();
            clientPublicKeyPackage.Body.Should().Be(clientPublicKey);
        }
    }
}
