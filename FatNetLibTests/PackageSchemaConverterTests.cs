﻿using FluentAssertions;
using Kolyhalov.FatNetLib.Initializers;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib;

public class PackageSchemaConverterTests
{
    private readonly JsonConverter[] _converters = { new PackageSchemaConverter(), new TypeConverter() };

    [Test]
    public void SerializeObject_PackageSchema_ReturnJson()
    {
        // Arrange
        var schema = new PackageSchema { { "Key", typeof(EndpointsBody) } };

        // Act
        string json = JsonConvert.SerializeObject(schema, _converters);

        // Assert
        json.Should().Be(@"{""Key"":""Kolyhalov.FatNetLib.Initializers.EndpointsBody,FatNetLib""}");
    }

    [Test]
    public void SerializeObject_NullPackageSchema_ReturnJson()
    {
        // Act
        string json = JsonConvert.SerializeObject(value: null, _converters);

        // Assert
        json.Should().Be("null");
    }

    [Test]
    public void DeserializeObject_Json_ReturnPackageSchema()
    {
        // Arrange
        const string json = @"{""Key"":""Kolyhalov.FatNetLib.Initializers.EndpointsBody,FatNetLib""}";

        // Act
        var schema = JsonConvert.DeserializeObject<PackageSchema>(json, _converters);

        // Assert
        schema.Should().BeEquivalentTo(new PackageSchema { { "Key", typeof(EndpointsBody) } });
    }

    [Test]
    public void DeserializeObject_NullJson_ReturnNull()
    {
        // Arrange
        const string json = "null";

        // Act
        var schema = JsonConvert.DeserializeObject<PackageSchema>(json, _converters);

        // Assert
        schema.Should().BeNull();
    }
}