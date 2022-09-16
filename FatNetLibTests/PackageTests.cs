using System;
using System.Collections.Generic;
using AutoFixture.NUnit3;
using FluentAssertions;
using Kolyhalov.FatNetLib;
using NUnit.Framework;

namespace FatNetLibTests;

public class PackageTests
{
    [Test, AutoData]
    public void SetField_SomeValue_ReturnSetValue(Package package, object value)
    {
        // Act
        package.SetField("CustomField", value);

        // Assert
        package.GetField<object>("CustomField").Should().Be(value);
    }

    [Test, AutoData]
    public void GetField_FieldWasNotSet_ReturnNull(Package package, object value)
    {
        // Assert
        package.GetField<object>("CustomField").Should().BeNull();
    }

    [Test, AutoData]
    public void SetField_SetNull_ReturnNull(Package package)
    {
        // Act
        package.SetField<object>("CustomField", null);

        // Assert
        package.GetField<object>("CustomField").Should().BeNull();
    }

    [Test, AutoData]
    public void RemoveField_FieldWasSet_ReturnNull(Package package, object value)
    {
        // Arrange
        package.SetField("CustomField", value);

        // Act
        package.RemoveField("CustomField");

        // Assert
        package.GetField<object>("CustomField").Should().BeNull();
    }

    [Test, AutoData]
    public void GetField_PrimitiveFieldWasNotSet_ReturnDefault(Package package)
    {
        // Assert
        package.GetField<bool>("CustomField").Should().BeFalse();
    }

    [Test, AutoData]
    public void Route_SetAndGetSomeValue_ReturnValue(Package package, string route)
    {
        // Act
        package.Route = route;

        // Assert
        package.Route.Should().Be(route);
    }

    [Test, AutoData]
    public void Body_SetAndGetSomeValue_ReturnValue(Package package, IDictionary<string, object> body)
    {
        // Act
        package.Body = body;

        // Assert
        package.Body.Should().BeSameAs(body);
    }

    [Test, AutoData]
    public void ExchangeId_SetAndGetSomeValue_ReturnValue(Package package, Guid exchangeId)
    {
        // Act
        package.ExchangeId = exchangeId;

        // Assert
        package.ExchangeId.Should().Be(exchangeId);
    }

    [Test, AutoData]
    public void ExchangeId_DefaultValue_ReturnNull(Package package)
    {
        // Act
        package.RemoveField(key: Package.Keys.ExchangeId);

        // Assert
        package.ExchangeId.Should().BeNull();
    }

    [Test, AutoData]
    public void IsResponse_SetAndGetSomeValue_ReturnValue(Package package)
    {
        // Act
        package.IsResponse = true;

        // Assert
        package.IsResponse.Should().BeTrue();
    }

    [Test, AutoData]
    public void Indexer_SetAndGetSomeValue_ReturnValue(Package package, object value)
    {
        // Act
        package["CustomField"] = value;

        // Assert
        package["CustomField"].Should().Be(value);
    }
}