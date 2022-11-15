using System;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Middlewares;
using Moq;
using NUnit.Framework;
using static System.Text.Encoding;
using static Kolyhalov.FatNetLib.Core.Tests.Utils.TestEncryptionUtils.Aes;

namespace Kolyhalov.FatNetLib.Core.Tests.Middlewares
{
    public class DecryptionMiddlewareTests
    {
        private static readonly byte[] Key =
        {
            205, 2, 185, 185, 8, 152, 7, 222, 124, 199, 229, 142, 67, 218, 66, 61, 54, 109, 36, 141, 163, 225, 86, 101,
            233,
            45, 188, 105, 155, 216, 57, 97
        }; // 32 bytes == 256 bits

        private static readonly byte[] EncryptedData = Encrypt(UTF8.GetBytes("test-data"), Key);

        private DecryptionMiddleware _middleware = null!;

        [SetUp]
        public void SetUp()
        {
            _middleware = new DecryptionMiddleware(maxNonDecryptionPeriod: 2, new Mock<ILogger>().Object);
        }

        [Test]
        public void Process_CorrectPackage_DecryptSerialized()
        {
            // Arrange
            var package = new Package
            {
                FromPeerId = 0,
                Serialized = EncryptedData
            };
            _middleware.RegisterPeer(peerId: 0, Key);

            // Act
            _middleware.Process(package);

            // Assert
            package.Serialized.Should().BeEquivalentTo(UTF8.GetBytes("test-data"));
        }

        [Test]
        public void Process_PackageWithoutFromPeerId_Throw()
        {
            // Arrange
            var package = new Package { Serialized = EncryptedData };
            _middleware.RegisterPeer(peerId: 0, Key);

            // Act
            Action act = () => _middleware.Process(package);

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage("FromPeerId field is missing");
        }

        [Test]
        public void Process_PackageWithoutSerialized_Throw()
        {
            // Arrange
            var package = new Package { FromPeerId = 0 };
            _middleware.RegisterPeer(peerId: 0, Key);

            // Act
            Action act = () => _middleware.Process(package);

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage("Serialized field is missing");
        }

        [Test]
        public void Process_WithoutPeerRegistration_SkipDecryption()
        {
            // Arrange
            var package = new Package
            {
                Serialized = EncryptedData,
                FromPeerId = 0
            };

            // Act
            _middleware.Process(package);

            // Assert
            package.Serialized.Should().BeEquivalentTo(EncryptedData);
        }

        [Test]
        public void Process_NonDecryptionPeriodIsOver_Throw()
        {
            // Arrange
            var package = new Package
            {
                Serialized = EncryptedData,
                FromPeerId = 0
            };
            _middleware.Process(package);
            _middleware.Process(package);

            // Act
            Action act = () => _middleware.Process(package);

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage("Decryption key was not found");
        }

        [Test]
        public void UnregisterPeer_RegisteredPeer_ThreatLikeNewUnknownPeer()
        {
            // Arrange
            var package = new Package
            {
                FromPeerId = 0,
                Serialized = EncryptedData
            };
            _middleware.RegisterPeer(peerId: 0, Key);
            _middleware.Process(package);
            package.Serialized = EncryptedData;

            // Act
            _middleware.UnregisterPeer(peerId: 0);

            // Assert
            _middleware.Process(package);
            package.Serialized.Should().BeEquivalentTo(EncryptedData);
        }

        [Test]
        public void Process_UnregisteredAndNonEncryptionPeriodIsOver_Throw()
        {
            // Arrange
            var package = new Package
            {
                FromPeerId = 0,
                Serialized = EncryptedData
            };
            _middleware.RegisterPeer(peerId: 0, Key);
            _middleware.Process(package);
            _middleware.UnregisterPeer(peerId: 0);
            package.Serialized = EncryptedData;
            _middleware.Process(package);
            _middleware.Process(package);

            // Act
            Action act = () => _middleware.Process(package);

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage("Decryption key was not found");
        }

        [Test]
        public void UnregisterPeer_UnknownPeer_Pass()
        {
            // Act
            Action act = () => _middleware.UnregisterPeer(peerId: 123);

            // Assert
            act.Should().NotThrow();
        }
    }
}
