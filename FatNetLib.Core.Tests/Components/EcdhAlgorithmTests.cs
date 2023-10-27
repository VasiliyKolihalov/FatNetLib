using System;
using AutoFixture.NUnit3;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Components;
using Kolyhalov.FatNetLib.Core.Exceptions;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Core.Tests.Components;

public class EcdhAlgorithmTests
{
    private EcdhAlgorithm _algorithm = null!;

    [SetUp]
    public void SetUp()
    {
        _algorithm = new EcdhAlgorithm();
    }

    [Test, AutoData]
    public void CalculateSharedSecret_InPairWithAnotherAlgorithm_ReturnSharedSecret(EcdhAlgorithm anotherAlgorithm)
    {
        // Act
        byte[] sharedSecret = _algorithm.CalculateSharedSecret(anotherAlgorithm.MyPublicKey);

        // Assert
        byte[] anotherSharedSecret = anotherAlgorithm.CalculateSharedSecret(_algorithm.MyPublicKey);
        sharedSecret.Should().BeEquivalentTo(anotherSharedSecret);
        sharedSecret.Length.Should().Be(32); // 256 bits == 32 bytes
    }

    [Test, AutoData]
    public void CalculateSharedSecret_Reuse_Throw(EcdhAlgorithm anotherAlgorithm1, EcdhAlgorithm anotherAlgorithm2)
    {
        // Arrange
        _algorithm.CalculateSharedSecret(anotherAlgorithm1.MyPublicKey);

        // Act
        Action act = () => _algorithm.CalculateSharedSecret(anotherAlgorithm2.MyPublicKey);

        // Assert
        act.Should().Throw<FatNetLibException>()
            .WithMessage("This class was not designed for reusing");
    }

    [Test]
    public void CalculateSharedSecret_ByInvalidPublicKey_Throw()
    {
        // Act
        Action act = () => _algorithm.CalculateSharedSecret(new byte[] { 12, 34, 56 });

        // Assert
        act.Should().Throw<FormatException>()
            .WithMessage("Invalid point encoding 12");
    }

    [Test, AutoData]
    public void CalculateSharedSecret_TwoPairsOfAlgorithms_ReturnNotSameSharedSecret(
        EcdhAlgorithm algorithm1,
        EcdhAlgorithm algorithm2,
        EcdhAlgorithm algorithm3,
        EcdhAlgorithm algorithm4)
    {
        // Act
        byte[] sharedSecret1 = algorithm1.CalculateSharedSecret(algorithm2.MyPublicKey);
        byte[] sharedSecret2 = algorithm3.CalculateSharedSecret(algorithm4.MyPublicKey);

        // Assert
        sharedSecret1.Should().NotBeEquivalentTo(sharedSecret2);
    }
}
