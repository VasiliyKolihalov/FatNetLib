using System;
using AutoFixture.NUnit3;
using FluentAssertions;
using Kolyhalov.FatNetLib;
using NUnit.Framework;

namespace FatNetLibTests;

public class RouteTests
{
    [Test]
    public void Route_NullRoute_Throw()
    {
        // Arrange
        string route = null!;

        // Act
        Action action = () => new Route(route);

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("Route is null or blank");
    }

    [Test]
    public void Route_BlankRoute_Throw()
    {
        // Act
        Action action = () => new Route("  ");

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("Route is null or blank");
    }

    [Test]
    public void Route_EmptyRoute_Throw()
    {
        // Act
        Action action = () => new Route(string.Empty);

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("Route is null or blank");
    }

    [Test]
    public void IsEmpty_NonEmptyRoute_ReturnFalse()
    {
        // Assert
        new Route("test/route").IsEmpty.Should().BeFalse();
    }

    [Test]
    public void Contains_RoutePart_ReturnTrue()
    {
        // Arrange
        var route = new Route("test/route");

        // Assert
        route.Contains("test").Should().BeTrue();
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
        // Arrange
        var route = "test/route";

        // Assert
        new Route(route).Should().BeEquivalentTo(new Route(route));
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
    public void Equals_AnotherType_ReturnFalse(string anotherTypePath)
    {
        // Assert
        new Route("test/route").Should().NotBeEquivalentTo(anotherTypePath);
    }

    [Test]
    public void Equals_DifferentValue_ReturnFalse()
    {
        // Arrange
        var route = "test/route";
        var anotherRoute = "some-route";

        // Assert
        new Route(anotherRoute).Should().NotBeEquivalentTo(new Route(route));
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
}