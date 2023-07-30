using System.Threading.Tasks;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Components.Server;
using Kolyhalov.FatNetLib.Core.Controllers.Server;
using Kolyhalov.FatNetLib.Core.Couriers;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Wrappers;
using Moq;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Core.Tests.Controllers.Server;

public class ServerEncryptionControllerTests
{
    private readonly Mock<INetPeer> _peer = new();
    private ServerEncryptionController _controller = null!;
    private Mock<IServerEncryptionService> _service = null!;
    private Mock<ICourier> _courier = null!;

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
    }

    [Test]
    public async Task ExchangePublicKeysAsync_CorrectCase_RegisterPeerWithSharedSecret()
    {
        // Act
        Package lastPackage = await _controller.ExchangePublicKeysAsync(_peer.Object, _courier.Object);

        // Assert
        _service.Verify(_ => _.ExchangePublicKeysAsync(_peer.Object, _courier.Object));
        _service.VerifyNoOtherCalls();
        lastPackage.Fields.Should().BeEmpty();
        lastPackage.NonSendingFields.Should().BeEmpty();
    }
}
