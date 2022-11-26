using AutoFixture.NUnit3;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Controllers.Client;
using Kolyhalov.FatNetLib.Core.Couriers;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Services.Client;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Wrappers;
using Moq;
using NUnit.Framework;
using static Moq.Times;

namespace Kolyhalov.FatNetLib.Core.Tests.Controllers.Client
{
    public class ClientEncryptionControllerTests
    {
        private readonly Mock<ISendingNetPeer> _peer = new Mock<ISendingNetPeer>();
        private ClientEncryptionController _controller = null!;
        private Mock<IClientEncryptionService> _service = null!;
        private Mock<ICourier> _courier = null!;
        private IDependencyContext _context = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _peer.Setup(_ => _.Id).Returns(0);
        }

        [SetUp]
        public void SetUp()
        {
            _service = new Mock<IClientEncryptionService>();
            _controller = new ClientEncryptionController(_service.Object);
            _courier = new Mock<ICourier>();
            _context = new DependencyContext();
            _context.Put(_courier.Object);
        }

        [Test, AutoData]
        public void ExchangePublicKeys_CorrectCase_RegisterPeerWithSharedSecret(
            byte[] serverPublicKey,
            byte[] clientPublicKey)
        {
            // Arrange
            var serverPublicKeyPackage = new Package
            {
                FromPeer = _peer.Object,
                Context = _context,
                Body = serverPublicKey
            };
            _service.Setup(_ => _.ExchangePublicKeys(It.IsAny<byte[]>(), It.IsAny<ISendingNetPeer>()))
                .Returns(clientPublicKey);

            // Act
            Package clientPublicKeyPackage = _controller.ExchangePublicKeys(serverPublicKeyPackage);

            // Assert
            _service.Verify(_ => _.ExchangePublicKeys(serverPublicKey, _peer.Object), Once);
            _service.VerifyNoOtherCalls();
            clientPublicKeyPackage.Body.Should().Be(clientPublicKey);
        }
    }
}
