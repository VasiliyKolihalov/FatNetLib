using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Controllers.Server;
using Kolyhalov.FatNetLib.Core.Services.Server;
using Kolyhalov.FatNetLib.Core.Storages;
using Moq;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Core.Tests.Controllers.Server
{
    public class ServerEncryptionControllerTests
    {
        private ServerEncryptionController _controller = null!;
        private Mock<IServerEncryptionService> _service = null!;
        private Mock<ICourier> _courier = null!;
        private IDependencyContext _context = null!;

        [SetUp]
        public void SetUp()
        {
            _service = new Mock<IServerEncryptionService>();
            _controller = new ServerEncryptionController(_service.Object);
            _courier = new Mock<ICourier>();
            _context = new DependencyContext();
            _context.Put(_ => _courier.Object);
        }

        [Test]
        public void ExchangePublicKeys_CorrectCase_RegisterPeerWithSharedSecret()
        {
            // Arrange
            var handshakePackage = new Package
            {
                FromPeerId = 0,
                Context = _context,
            };

            // Act
            Package lastPackage = _controller.ExchangePublicKeys(handshakePackage);

            // Assert
            _service.Verify(_ => _.ExchangePublicKeys(0, _courier.Object));
            _service.VerifyNoOtherCalls();
            lastPackage.Should().BeEquivalentTo(new Package());
        }
    }
}
