using AutoFixture.NUnit3;
using FluentAssertions;
using Kolyhalov.FatNetLib.Microtypes;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib;

public class DependencyContextTests
{
    private DependencyContext _context = null!;

    [SetUp]
    public void SetUp()
    {
        _context = new DependencyContext();
    }

    [Test, AutoData]
    public void Put_ByStringId_ReturnDependency(object dependency)
    {
        // Act
        _context.Put("some-id", _ => dependency);

        // Assert
        _context.Get<object>("some-id").Should().Be(dependency);
    }

    [Test, AutoData]
    public void Put_ReplacingOld_ReturnDependency(object oldDependency, object newDependency)
    {
        // Assert
        _context.Put("some-id", _ => oldDependency);

        // Act
        _context.Put("some-id", _ => newDependency);

        // Assert
        _context.Get<object>("some-id").Should().Be(newDependency);
    }

    [Test, AutoData]
    public void Put_ByType_ReturnDependency(object dependency)
    {
        // Act
        _context.Put(_ => dependency);

        // Assert
        _context.Get<object>().Should().Be(dependency);
    }

    [Test, AutoData]
    public void CopyReference_ObjectWithInterface_ReturnDependency(Route dependency)
    {
        // Arrange
        _context.Put(_ => dependency);

        // Act
        _context.CopyReference(typeof(Route), typeof(object));

        // Assert
        _context.Get<Route>().Should().Be(dependency);
        _context.Get<object>().Should().Be(dependency);
    }
}