﻿using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Couriers;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Services;
using Kolyhalov.FatNetLib.Core.Services.Server;
using Kolyhalov.FatNetLib.Core.Wrappers;
using Moq;
using NUnit.Framework;
using static Moq.Times;

namespace Kolyhalov.FatNetLib.Core.Tests.Services
{
    public class ServerEncryptionServiceTests
    {
        private readonly Mock<ISendingNetPeer> _peer = new Mock<ISendingNetPeer>();
        private ServerEncryptionService _service = null!;
        private Mock<IEncryptionPeerRegistry> _encryptionRegistry = null!;
        private Mock<IEncryptionPeerRegistry> _decryptionRegistry = null!;
        private Mock<ICourier> _courier = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _peer.Setup(_ => _.Id).Returns(0);
        }

        [SetUp]
        public void SetUp()
        {
            _encryptionRegistry = new Mock<IEncryptionPeerRegistry>();
            _decryptionRegistry = new Mock<IEncryptionPeerRegistry>();
            _service = new ServerEncryptionService(_encryptionRegistry.Object, _decryptionRegistry.Object);
            _courier = new Mock<ICourier>();
        }

        [Test]
        public void ExchangePublicKeys_CorrectCase_RegisterPeerWithSharedSecret()
        {
            // Arrange
            var clientAlgorithm = new EcdhAlgorithm();

            byte[] expectedSharedSecret = null!;
            _courier.Setup(_ => _.Send(It.IsAny<Package>()))
                .Callback<Package>(package =>
                    expectedSharedSecret = clientAlgorithm.CalculateSharedSecret(package.GetBodyAs<byte[]>()))
                .Returns(new Package
                {
                    Body = clientAlgorithm.MyPublicKey,
                    FromPeer = _peer.Object
                });

            byte[] actualSharedSecret = null!;
            _encryptionRegistry.Setup(_ => _.RegisterPeer(_peer.Object, It.IsAny<byte[]>()))
                .Callback<INetPeer, byte[]>((_, key) => actualSharedSecret = key);

            // Act
            _service.ExchangePublicKeys(_peer.Object, _courier.Object);

            // Assert
            _courier.Verify(_ => _.Send(It.IsAny<Package>()), Once);
            _courier.VerifyNoOtherCalls();
            _encryptionRegistry.Verify(_ => _.RegisterPeer(_peer.Object, actualSharedSecret), Once);
            _encryptionRegistry.VerifyNoOtherCalls();
            _decryptionRegistry.Verify(_ => _.RegisterPeer(_peer.Object, actualSharedSecret), Once);
            _decryptionRegistry.VerifyNoOtherCalls();
            actualSharedSecret.Should().BeEquivalentTo(expectedSharedSecret);
        }
    }
}
