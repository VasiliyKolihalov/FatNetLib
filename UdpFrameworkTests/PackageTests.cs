using AutoFixture.NUnit3;
using FluentAssertions;
using Kolyhalov.UdpFramework;
using NUnit.Framework;

namespace UdpFrameworkTests;

public class PackageTests
{
    [Test, AutoData]
    public void Constructor_CopyPackage_CopyHasSameFields(Package source)
    {
        // Act
        var copy = new Package(source);

        // Assert
        copy.Should().BeEquivalentTo(source);
    }
}