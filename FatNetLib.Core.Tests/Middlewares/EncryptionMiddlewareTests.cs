using System;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Middlewares;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Wrappers;
using Moq;
using NUnit.Framework;
using static System.Text.Encoding;
using static Kolyhalov.FatNetLib.Core.Tests.Utils.TestEncryptionUtils.Aes;

namespace Kolyhalov.FatNetLib.Core.Tests.Middlewares
{
    public class EncryptionMiddlewareTests
    {
        private readonly byte[] _key =
        {
            141, 163, 225, 86, 101, 233, 45, 188, 105, 155, 216, 57, 97, 205, 2, 185, 185, 8, 152, 7, 222, 124, 199,
            229,
            142, 67, 218, 66, 61, 54, 109, 36
        }; // 32 bytes == 256 bits

        private readonly Mock<INetPeer> _peer = new Mock<INetPeer>();
        private EncryptionMiddleware _middleware = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _peer.Setup(_ => _.Id).Returns(0);
        }

        [SetUp]
        public void SetUp()
        {
            _middleware = new EncryptionMiddleware(maxNonEncryptionPeriod: 2, new Mock<ILogger>().Object);
        }

        [Test]
        public void Process_CorrectPackage_EncryptSerialized()
        {
            // Arrange
            var package = new Package
            {
                ToPeer = _peer.Object,
                Serialized = UTF8.GetBytes("test-data")
            };
            _middleware.RegisterPeer(_peer.Object, _key);

            // Act
            _middleware.Process(package);

            // Assert
            Decrypt(package.Serialized, _key)
                .Should().BeEquivalentTo(UTF8.GetBytes("test-data"));
        }

        [Test]
        public void Process_PackageWithSkipEncryption_SkipEncryption()
        {
            // Arrange
            var package = new Package
            {
                Serialized = UTF8.GetBytes("test-data"),
                ToPeer = _peer.Object
            };
            package.SetNonSendingField("SkipEncryption", value: true);
            _middleware.RegisterPeer(_peer.Object, _key);

            // Act
            _middleware.Process(package);

            // Assert
            package.Serialized.Should().BeEquivalentTo(UTF8.GetBytes("test-data"));
        }

        [Test]
        public void Process_PackageWithoutToPeerId_Throw()
        {
            // Arrange
            var package = new Package { Serialized = UTF8.GetBytes("test-data") };
            _middleware.RegisterPeer(_peer.Object, _key);

            // Act
            Action act = () => _middleware.Process(package);

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage("Package must contain ToPeer field");
        }

        [Test]
        public void Process_PackageWithoutSerialized_Throw()
        {
            // Arrange
            var package = new Package { ToPeer = _peer.Object };
            _middleware.RegisterPeer(_peer.Object, _key);

            // Act
            Action act = () => _middleware.Process(package);

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage("Package must contain Serialized field");
        }

        [Test]
        public void Process_WithoutPeerRegistration_SkipEncryption()
        {
            // Arrange
            var package = new Package
            {
                Serialized = UTF8.GetBytes("test-data"),
                ToPeer = _peer.Object
            };

            // Act
            _middleware.Process(package);

            // Assert
            package.Serialized.Should().BeEquivalentTo(UTF8.GetBytes("test-data"));
        }

        [Test]
        public void Process_NonEncryptionPeriodIsOver_Throw()
        {
            // Arrange
            var package = new Package
            {
                Serialized = UTF8.GetBytes("test-data"),
                ToPeer = _peer.Object
            };
            _middleware.Process(package);
            _middleware.Process(package);

            // Act
            Action act = () => _middleware.Process(package);

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage("Encryption key was not found");
        }

        [Test]
        public void UnregisterPeer_RegisteredPeer_ThreatLikeNewUnknownPeer()
        {
            // Arrange
            var package = new Package
            {
                ToPeer = _peer.Object,
                Serialized = UTF8.GetBytes("test-data")
            };
            _middleware.RegisterPeer(_peer.Object, _key);
            _middleware.Process(package);
            package.Serialized = UTF8.GetBytes("test-data");

            // Act
            _middleware.UnregisterPeer(_peer.Object);

            // Assert
            _middleware.Process(package);
            package.Serialized.Should().BeEquivalentTo(UTF8.GetBytes("test-data"));
        }

        [Test]
        public void Process_UnregisteredAndNonEncodingPeriodIsOver_Throw()
        {
            // Arrange
            var package = new Package
            {
                ToPeer = _peer.Object,
                Serialized = UTF8.GetBytes("test-data")
            };
            _middleware.RegisterPeer(_peer.Object, _key);
            _middleware.Process(package);
            _middleware.UnregisterPeer(_peer.Object);
            package.Serialized = UTF8.GetBytes("test-data");
            _middleware.Process(package);
            _middleware.Process(package);

            // Act
            Action act = () => _middleware.Process(package);

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage("Encryption key was not found");
        }

        [Test]
        public void UnregisterPeer_UnknownPeer_Pass()
        {
            // Act
            Action act = () => _middleware.UnregisterPeer(Mock.Of<INetPeer>());

            // Assert
            act.Should().NotThrow();
        }
    }
}
