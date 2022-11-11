using System;
using AutoFixture.NUnit3;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Microtypes;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Tests.Microtypes;

public class RouteTests
{
    [Test]
    public void Route_RouteFromNull_Throw()
    {
        // Act
        Action action = () => new Route(null!);

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("Route is null or blank");
    }

    [Test]
    public void Route_RouteFromBlank_Throw()
    {
        // Act
        Action action = () => new Route("  ");

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("Route is null or blank");
    }

    [Test]
    public void Route_RouteFromEmpty_Throw()
    {
        // Act
        Action action = () => new Route(string.Empty);

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("Route is null or blank");
    }

    [Test]
    public void Route_RouteWithInvalidSymbol()
    {
        Action action = () => new Route("test/route" + Environment.NewLine);
        action.Should().Throw<ArgumentException>().WithMessage("Invalid symbol in route: \r");
    }

    [Test]
    public void IsEmpty_NonEmptyRoute_ReturnFalse()
    {
        // Assert
        new Route("test/route").IsEmpty.Should().BeFalse();
    }

    [Test]
    public void Plus_AnotherRoute_ReturnIncreasedRoute()
    {
        // Arrange
        var route = new Route("test");

        // Act
        route += new Route("route");

        // Assert
        route.Should().BeEquivalentTo(new Route("test/route"));
    }

    [Test]
    public void Plus_RouteAsString_ReturnIncreasedRoute()
    {
        // Arrange
        var route = new Route("test");

        // Act
        route += "route";

        // Assert
        route.Should().BeEquivalentTo(new Route("test/route"));
    }

    [Test]
    public void ToString_ReturnStringRoute()
    {
        // Arrange
        var route = new Route("test/route");

        // Assert
        route.ToString().Should().Be("test/route");
    }

    [Test]
    public void Equals_SameValue_ReturnTrue()
    {
        // Assert
        new Route("test/route").Should().BeEquivalentTo(new Route("test/route"));
    }

    [Test]
    public void Equals_SameReference_ReturnTrue()
    {
        // Arrange
        var route = new Route("test/route");

        // Assert
        route.Should().BeEquivalentTo(route);
    }

    [Test]
    public void Equals_Null_ReturnFalse()
    {
        // Arrange
        Route route = null!;

        // Assert
        new Route("test/route").Should().NotBeEquivalentTo(route);
    }

    [Test, AutoData]
    public void Equals_AnotherType_ReturnFalse(string anotherTypeRoute)
    {
        // Assert
        new Route("test/route").Should().NotBeEquivalentTo(anotherTypeRoute);
    }

    [Test]
    public void Equals_DifferentValue_ReturnFalse()
    {
        // Assert
        new Route("test/route").Should().NotBeEquivalentTo(new Route("another/route"));
    }

    [Test]
    public void Equals_ShuffledSegments_ReturnFalse()
    {
        // Arrange
        var route1 = new Route("segment-1/segment-2");
        var route2 = new Route("segment-2/segment-1");

        // Assert
        route1.Should().NotBeEquivalentTo(route2);
    }

    [Test]
    public void IsNotEmpty_SomeRoutes_ReturnCorrectValues()
    {
        // Assert
        new Route("some/route").IsNotEmpty.Should().BeTrue();
        Route.Empty.IsNotEmpty.Should().BeFalse();
    }
}
