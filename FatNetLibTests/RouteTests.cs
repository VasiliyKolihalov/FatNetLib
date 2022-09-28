using System;
using AutoFixture.NUnit3;
using FluentAssertions;
using Kolyhalov.FatNetLib;
using NUnit.Framework;

namespace FatNetLibTests;

public class RouteTests
{
    [Test]
    public void Route_CorrectRoute_SetValue()
    {
        // Act
        var route = new Route("start/end");

        // Assert
        route.Value.Should().BeEquivalentTo(new Route("start/end").Value);
    }

    [Test]
    public void Route_DefaultConstructor_SetValue()
    {
        // Act
        var route = new Route();

        // Assert
        route.Value.Should().NotBeNull();
        route.IsEmpty.Should().BeTrue();
    }

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
    public void Route_OnlySlashesRoute_Throw()
    {
        // Act
        Action action = () => new Route("/");

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("Route cannot contain only slashes");
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
        new Route("start/end").IsEmpty.Should().BeFalse();
    }

    [Test]
    public void Contains_RoutePart_ReturnTrue()
    {
        // Arrange
        var route = new Route("start/end");

        // Assert
        route.Contains("start").Should().BeTrue();
    }

    [Test]
    public void Plus_AnotherRoute_ReturnIncreasedRoute()
    {
        // Arrange
        var route = new Route("start");

        // Act
        route += new Route("end");

        // Assert
        route.Should().BeEquivalentTo(new Route("start/end"));
    }

    [Test]
    public void Plus_RoutePart_ReturnIncreasedRoute()
    {
        // Arrange
        var route = new Route("start");

        // Act
        route += "end";

        // Assert
        route.Should().BeEquivalentTo(new Route("start/end"));
    }

    [Test]
    public void ToString_ReturnStringRoute()
    {
        // Arrange
        var route = new Route("start/end");

        // Assert
        route.ToString().Should().Be("start/end/");
    }

    [Test]
    public void Equals_SameValue_ReturnTrue()
    {
        // Arrange
        var route = "start-route/route\\end-route";

        // Assert
        new Route(route).Should().BeEquivalentTo(new Route(route));
    }

    [Test]
    public void Equals_SameReference_ReturnTrue()
    {
        // Arrange
        var route = new Route();

        // Assert
        route.Should().BeEquivalentTo(route);
    }

    [Test]
    public void Equals_Null_ReturnFalse()
    {
        // Arrange
        Route route = null!;
        
        // Assert
        new Route().Should().NotBeEquivalentTo(route);
    }

    [Test, AutoData]
    public void Equals_AnotherType_ReturnFalse(string route)
    {
        // Assert
        new Route().Should().NotBeEquivalentTo(route);
    }

    [Test]
    public void Equals_DifferentValue_ReturnFalse()
    {
        // Arrange
        var route = "start-route/route\\end-route";
        var anotherRoute = "route";

        // Assert
        new Route(anotherRoute).Should().NotBeEquivalentTo(new Route(route));
    }
}