using System;
using System.Collections.Generic;
using AutoFixture.NUnit3;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Tests;

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

    [Test]
    public void Contains_KeyIsSet_ReturnTrue()
    {
        // Arrange
        var schema = new PackageSchema { { "Key", typeof(int) } };

        // Assert
        schema.ContainsKey("Key").Should().BeTrue();
    }

    [Test]
    public void Contains_KeyIsNotSet_ReturnTrue()
    {
        // Arrange
        var schema = new PackageSchema { { "Key", typeof(int) } };

        // Assert
        schema.ContainsKey("AnotherKey").Should().BeFalse();
    }

    [Test]
    public void Patch()
    {
        // Arrange
        var schema = new PackageSchema { { "Key1", typeof(int) }, { "Key2", typeof(long) } };
        var schemaPatch = new PackageSchema { { "Key2", typeof(string) }, { "Key3", typeof(double) } };

        // Act
        schema.Patch(schemaPatch);

        // Assert
        schemaPatch.Should().NotBeSameAs(schema);
        schema.Should().BeEquivalentTo(new PackageSchema
            { { "Key1", typeof(int) }, { "Key2", typeof(string) }, { "Key3", typeof(double) } });
    }
}
