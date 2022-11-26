using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Controllers.Server;
using Kolyhalov.FatNetLib.Core.Couriers;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Services.Server;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Wrappers;
using Moq;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Core.Tests.Controllers.Server
{
    public class ServerEncryptionControllerTests
    {
        private readonly Mock<ISendingNetPeer> _peer = new Mock<ISendingNetPeer>();
        private ServerEncryptionController _controller = null!;
        private Mock<IServerEncryptionService> _service = null!;
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
            _service = new Mock<IServerEncryptionService>();
            _controller = new ServerEncryptionController(_service.Object);
            _courier = new Mock<ICourier>();
            _context = new DependencyContext();
            _context.Put(_courier.Object);
        }

        [Test]
        public void ExchangePublicKeys_CorrectCase_RegisterPeerWithSharedSecret()
        {
            // Arrange
            var handshakePackage = new Package
            {
                FromPeer = _peer.Object,
                Context = _context
            };

            // Act
            Package lastPackage = _controller.ExchangePublicKeys(handshakePackage);

            // Assert
            _service.Verify(_ => _.ExchangePublicKeys(_peer.Object, _courier.Object));
            _service.VerifyNoOtherCalls();
            lastPackage.Should().BeEquivalentTo(new Package());
        }
    }
}
