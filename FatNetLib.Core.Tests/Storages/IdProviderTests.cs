using System;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Storages;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Core.Tests.Storages;

public class IdProviderTests
{
    private readonly IdProvider _idProvider = new();

    [Test]
    public void GetId_ByNewObject_ReturnRandomId()
    {
        // Act
        Guid id = _idProvider.GetId(new object());

        id.Should().NotBeEmpty();
    }

    [Test]
    public void GetId_ByTwoObjects_ReturnDifferentIds()
    {
        // Arrange
        Guid firstId = _idProvider.GetId(new object());

        // Act
        Guid secondId = _idProvider.GetId(new object());

        // Assert
        firstId.Should().NotBe(secondId);
    }

    [Test]
    public void GetId_BySameObjectTwice_ReturnSameId()
    {
        // Arrange
        var commonObject = new object();
        Guid firstId = _idProvider.GetId(commonObject);

        // Act
        Guid secondId = _idProvider.GetId(commonObject);

        // Assert
        firstId.Should().Be(secondId);
    }
}
