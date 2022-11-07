using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using static System.Text.Encoding;
using static Kolyhalov.FatNetLib.Utils.TestEncryptionUtils.Aes;

namespace Kolyhalov.FatNetLib.Modules.Encryption;

public class EncryptionMiddlewareTests
{
    private readonly byte[] _key =
    {
        141, 163, 225, 86, 101, 233, 45, 188, 105, 155, 216, 57, 97, 205, 2, 185, 185, 8, 152, 7, 222, 124, 199, 229,
        142, 67, 218, 66, 61, 54, 109, 36
    }; // 32 bytes == 256 bits

    private EncryptionMiddleware _middleware = null!;

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
            ToPeerId = 0,
            Serialized = UTF8.GetBytes("test-data")
        };
        _middleware.RegisterPeer(peerId: 0, _key);

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
            ToPeerId = 0
        };
        package.SetNonSendingField("SkipEncryption", value: true);
        _middleware.RegisterPeer(peerId: 0, _key);

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
        _middleware.RegisterPeer(peerId: 0, _key);

        // Act
        Action act = () => _middleware.Process(package);

        // Assert
        act.Should().Throw<FatNetLibException>()
            .WithMessage("ToPeerId field is missing");
    }

    [Test]
    public void Process_PackageWithoutSerialized_Throw()
    {
        // Arrange
        var package = new Package { ToPeerId = 0 };
        _middleware.RegisterPeer(peerId: 0, _key);

        // Act
        Action act = () => _middleware.Process(package);

        // Assert
        act.Should().Throw<FatNetLibException>()
            .WithMessage("Serialized field is missing");
    }

    [Test]
    public void Process_WithoutPeerRegistration_SkipEncryption()
    {
        // Arrange
        var package = new Package
        {
            Serialized = UTF8.GetBytes("test-data"),
            ToPeerId = 0
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
            ToPeerId = 0
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
            ToPeerId = 0,
            Serialized = UTF8.GetBytes("test-data")
        };
        _middleware.RegisterPeer(peerId: 0, _key);
        _middleware.Process(package);
        package.Serialized = UTF8.GetBytes("test-data");

        // Act
        _middleware.UnregisterPeer(peerId: 0);

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
            ToPeerId = 0,
            Serialized = UTF8.GetBytes("test-data")
        };
        _middleware.RegisterPeer(peerId: 0, _key);
        _middleware.Process(package);
        _middleware.UnregisterPeer(peerId: 0);
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
        Action act = () => _middleware.UnregisterPeer(peerId: 123);

        // Assert
        act.Should().NotThrow();
    }
}
