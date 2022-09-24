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
    public void SetField_SomeValue_ValueInFields(Package package, object value)
    {
        // Act
        package.SetField("CustomField", value);

        // Assert
        package.Fields["CustomField"].Should().Be(value);
    }

    [Test, AutoData]
    public void GetField_SomeValueInFields_ReturnValue(Package package, object value)
    {
        // Arrange
        package.Fields["CustomField"] = value;
        
        // Act
        var returnedValue = package.GetField<object>("CustomField");

        // Assert
        returnedValue.Should().Be(value);
    }
    
    [Test, AutoData]
    public void GetField_ObjectWasNotSet_ReturnNull(Package package, object value)
    {
        // Assert
        package.GetField<object>("CustomField").Should().BeNull();
    }
    
    [Test, AutoData]
    public void GetField_StructWasNotSet_ReturnEmptyStruct(Package package, object value)
    {
        // Assert
        package.GetField<Guid>("CustomField").Should().BeEmpty();
    }
    
    [Test, AutoData]
    public void GetField_PrimitiveWasNotSet_ReturnDefault(Package package, object value)
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
    
    [Test, AutoData]
    public void SetPrivateField_SomeValue_SetValue(Package package, object value)
    {
        // Act
        package.SetPrivateField("CustomPrivateField", value);

        // Assert
        package.PrivateFields["CustomPrivateField"].Should().Be(value);
    }

    [Test, AutoData]
    public void GetPrivateField_SomeValueInPrivateFields_ReturnValue(Package package, object value)
    {
        // Arrange
        package.PrivateFields["CustomPrivateField"] = value;
        
        // Act
        var returnedValue = package.GetPrivateField<object>("CustomPrivateField");

        // Assert
        returnedValue.Should().Be(value);
    }
    
    [Test, AutoData]
    public void GetPrivateField_ObjectWasNotSet_ReturnNull(Package package, object value)
    {
        // Assert
        package.GetPrivateField<object>("CustomPrivateField").Should().BeNull();
    }
    
    [Test, AutoData]
    public void GetPrivateField_StructWasNotSet_ReturnEmptyStruct(Package package, object value)
    {
        // Assert
        package.GetPrivateField<Guid>("CustomPrivateField").Should().BeEmpty();
    }
    
    [Test, AutoData]
    public void GetPrivateField_PrimitiveWasNotSet_ReturnDefault(Package package, object value)
    {
        // Assert
        package.GetPrivateField<bool>("CustomPrivateField").Should().BeFalse();
    }
    
    [Test, AutoData]
    public void Serialized_SetAndGetSomeValue_ReturnValue(Package package)
    {
        // Act
        package.Serialized = "serialized-package";

        // Assert
        package.Serialized.Should().Be("serialized-package");
    }
    
    [Test, AutoData]
    public void Serialized_SetAndGetSomeValue_ReturnValue(Package package, PackageSchema packageSchema)
    {
        // Act
        package.Schema = packageSchema;

        // Assert
        package.Schema.Should().BeSameAs(packageSchema);
    }
}