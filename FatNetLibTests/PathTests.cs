using System;
using System.Collections.Generic;
using AutoFixture.NUnit3;
using FluentAssertions;
using Kolyhalov.FatNetLib;
using NUnit.Framework;

namespace FatNetLibTests;

public class PathTests
{
    [Test]
    public void Path_CorrectRoute_SetValue()
    {
         // Act
         var path = new Path("start-route/route\\end-route");
         
         // Assert
         path.Value.Should().BeEquivalentTo(new List<string> {"start-route", "route", "end-route"});
    }

    [Test]
    public void Path_RouteList_SetValue()
    {
        // Arrange
        List<string> route = new List<string> { "start-route", "route", "end-route" };

        // Act
        var path = new Path(route);

        // Assert
        path.Value.Should().BeEquivalentTo(route);
    }

    [Test]
    public void Path_DefaultConstructor_SetValue()
    {
        // Act
        var path = new Path();

        // Assert
        path.Value.Should().NotBeNull();
    }

    [Test]
    public void Path_NullRoute_Throw()
    {
        // Arrange
        string route = null!;
        
        // Act
        Action action = () => new Path(route);

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("Path cannot be is null or blank");
    }
    
    [Test]
    public void Path_BlankRoute_Throw()
    {
        // Act
        Action action = () => new Path("  ");
        
        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("Path cannot be is null or blank");
    }
    
    [Test]
    public void Path_OnlySlashesRoute_Throw()
    {
        // Act
        Action action = () => new Path("/\\");

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("Path cannot contain only slashes");
    }
    
    [Test]
    public void Path_EmptyRoute_Throw()
    {
        // Act
        Action action = () => new Path(string.Empty);

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("Path cannot be is null or blank");
    }

    [Test]
    public void Path_EmptyRouteList_Throw()
    {
        // Act
        Action action = () => new Path(new List<string>());

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("Path cannot be empty");
    }
    
    [Test]
    public void Path_RouteListWithSlashes_Throw()
    {
        // Arrange
        var route = new List<string> { "/", "\\" };

        // Act
        Action action = () => new Path(route);

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("Path cannot contain slashes");
    }
    
    [Test]
    public void Equals_SameValue_ReturnTrue()
    {
        // Arrange
        var route = "start-route/route\\end-route";
        
        // Act
        bool result = new Path(route).Equals(new Path(route));

        // Assert
        result.Should().BeTrue();
    }
    
    [Test]
    public void Equals_SameReference_ReturnTrue()
    {
        // Arrange
        var path = new Path();

        // Act
        bool result = path.Equals(path);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void Equals_Null_ReturnFalse()
    {
        // Act
        bool result = new Path().Equals(null);

        // Assert
        result.Should().BeFalse();
    }
    
    [Test, AutoData]
    public void Equals_AnotherType_ReturnFalse(object path)
    {
        // Act
        bool result = new Path().Equals(path);

        // Assert
        result.Should().BeFalse();
    }
    
    [Test]
    public void Equals_DifferentValue_ReturnFalse()
    {
        // Arrange
        var route = "start-route/route\\end-route";
        var anotherRoute = "route";

        // Act
        bool result = new Path(anotherRoute).Equals(new Path(route));

        // Assert
        result.Should().BeFalse();
    }
}