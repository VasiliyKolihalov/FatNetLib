using System;
using System.Collections.Generic;
using AutoFixture.NUnit3;
using FluentAssertions;
using Kolyhalov.FatNetLib;
using NUnit.Framework;

namespace FatNetLibTests;

public class PackageSchemaTests
{
    [Test, AutoData]
    public void Indexer_SetSomeValue_ReturnValue(PackageSchema schema, Type value)
    {
        // Act
        schema["CustomField"] = value;

        // Assert
        schema["CustomField"].Should().Be(value);
    }

    [Test, AutoData]
    public void Indexer_GetFromEmptySchema_Throw(Type value)
    {
        // Assert
        var schema = new PackageSchema();
        
        // Act
        Action act = () => schema["CustomField"].Should().Be(value);

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("The given key 'CustomField' was not present in the dictionary.");
    }
    
    [Test, AutoData]
    public void Add_SomeTypes_ValidSchema(Type value)
    {
        // Act
        var schema = new PackageSchema
        {
            { "CustomField", typeof(Guid) }
        };
        
        // Assert
        schema.Should().Equal(elements: new KeyValuePair<string, Type>("CustomField", typeof(Guid)));
    }
}