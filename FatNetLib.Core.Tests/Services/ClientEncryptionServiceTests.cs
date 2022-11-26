using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Couriers;
using Kolyhalov.FatNetLib.Core.Services;
using Kolyhalov.FatNetLib.Core.Services.Client;
using Kolyhalov.FatNetLib.Core.Storages;
using Moq;
using NUnit.Framework;
using static Moq.Times;

namespace Kolyhalov.FatNetLib.Core.Tests.Services
{
    public class ClientEncryptionServiceTests
    {
        private ClientEncryptionService _service = null!;
        private Mock<IEncryptionPeerRegistry> _encryptionRegistry = null!;
        private Mock<IEncryptionPeerRegistry> _decryptionRegistry = null!;
        private Mock<ICourier> _courier = null!;
        private IDependencyContext _context = null!;

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
}
