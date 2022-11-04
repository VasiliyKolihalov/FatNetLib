using System;
using FluentAssertions;
using Kolyhalov.FatNetLib.Initializers;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib;

public class TypeConverterTests
{
    private readonly JsonConverter[] _converters = { new TypeConverter() };

    [Test]
    public void SerializeObject_Type_ReturnJson()
    {
        // Arrange
        Type type = typeof(EndpointsBody);

        // Act
        string json = JsonConvert.SerializeObject(type, _converters);

        // Assert
        json.Should().Be(@"""Kolyhalov.FatNetLib.Initializers.EndpointsBody,FatNetLib""");
    }

    [Test]
    public void SerializeObject_NullType_ReturnJson()
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
        const string json = @"""Kolyhalov.FatNetLib.Initializers.EndpointsBody,FatNetLib""";

        // Act
        var type = JsonConvert.DeserializeObject<Type>(json, _converters);

        // Assert
        type.Should().Be(typeof(EndpointsBody));
    }

    [Test]
    public void DeserializeObject_NullJson_ReturnNull()
    {
        // Arrange
        const string json = "null";

        // Act
        var type = JsonConvert.DeserializeObject<Type>(json, _converters);

        // Assert
        type.Should().BeNull();
    }
}
