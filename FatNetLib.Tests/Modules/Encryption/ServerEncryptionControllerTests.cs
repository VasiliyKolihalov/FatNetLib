using FluentAssertions;
using Kolyhalov.FatNetLib.Core;
using Kolyhalov.FatNetLib.Core.Modules.Encryption;
using Moq;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Tests.Modules.Encryption;

public class ServerEncryptionControllerTests
{
    private ServerEncryptionController _controller = null!;
    private Mock<IServerEncryptionService> _service = null!;
    private Mock<IClient> _client = null!;
    private IDependencyContext _context = null!;

    [SetUp]
    public void SetUp()
    {
        _service = new Mock<IServerEncryptionService>();
        _controller = new ServerEncryptionController(_service.Object);
        _client = new Mock<IClient>();
        _context = new DependencyContext();
        _context.Put(_ => _client.Object);
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
        _service.Verify(_ => _.ExchangePublicKeys(0, _client.Object));
        _service.VerifyNoOtherCalls();
        lastPackage.Should().BeEquivalentTo(new Package());
    }
}
