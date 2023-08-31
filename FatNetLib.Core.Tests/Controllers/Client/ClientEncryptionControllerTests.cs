using System;
using AutoFixture.NUnit3;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Components.Client;
using Kolyhalov.FatNetLib.Core.Controllers.Client;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Wrappers;
using Moq;
using NUnit.Framework;
using static Moq.Times;

namespace Kolyhalov.FatNetLib.Core.Tests.Controllers.Client;

public class ClientEncryptionControllerTests
{
    private readonly Mock<INetPeer> _peer = new();
    private ClientEncryptionController _controller = null!;
    private Mock<IClientEncryptionService> _service = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _peer.Setup(_ => _.Id).Returns(Guid.NewGuid());
    }

    [SetUp]
    public void SetUp()
    {
        _service = new Mock<IClientEncryptionService>();
        _controller = new ClientEncryptionController(_service.Object);
    }

    [Test, AutoData]
    public void ExchangePublicKeys_CorrectCase_RegisterPeerWithSharedSecret(
        byte[] serverPublicKey,
        byte[] clientPublicKey)
    {
        // Arrange
        _service.Setup(_ => _.ExchangePublicKeys(It.IsAny<byte[]>(), It.IsAny<INetPeer>()))
            .Returns(clientPublicKey);

        // Act
        Package clientPublicKeyPackage = _controller.ExchangePublicKeys(serverPublicKey, _peer.Object);

        // Assert
        _service.Verify(_ => _.ExchangePublicKeys(serverPublicKey, _peer.Object), Once);
        _service.VerifyNoOtherCalls();
        clientPublicKeyPackage.Body.Should().Be(clientPublicKey);
    }
}
