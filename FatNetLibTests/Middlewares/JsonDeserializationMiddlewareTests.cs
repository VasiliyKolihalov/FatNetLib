using System;
using System.Collections.Generic;
using FluentAssertions;
using Kolyhalov.FatNetLib.Microtypes;
using Newtonsoft.Json;
using NUnit.Framework;
using static System.Text.Encoding;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace Kolyhalov.FatNetLib.Middlewares;

public class JsonDeserializationMiddlewareTests
{
    private readonly byte[] _jsonPackage = UTF8.GetBytes(@"{
        ""ExchangeId"": ""41f2d214-5d66-4c78-9e97-c03107cec3fd"",
        ""Body"": {""Endpoints"": [{""Route"": ""some-route"",""DeliveryMethod"": 1}]}
    }");

    private static readonly JsonSerializer JsonSerializer = JsonSerializer.Create(
        new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> { new RouteConverter() }
        });

    private static readonly JsonDeserializationMiddleware Middleware = new(JsonSerializer);

    [Test]
    public void SendPackage_NullPackage_Throw()
    {
        var package = new Package
        {
            Serialized = _jsonPackage,
            Schema = new PackageSchema
            {
                { nameof(Package.Body), typeof(IDictionary<string, object>) },
                { nameof(Package.ExchangeId), typeof(Guid) }
            }
        };
        Middleware.Process(package);
    }

    [Test]
    public void Process_WithoutSerialized_Throw()
    {
        // Arrange
        var package = new Package
        {
            Schema = new PackageSchema
            {
                { nameof(Package.Body), typeof(IDictionary<string, object>) },
                { nameof(Package.ExchangeId), typeof(Guid) }
            }
        };

        // Act
        Action act = () => Middleware.Process(package);

        // Assert
        act.Should().Throw<FatNetLibException>()
            .WithMessage("Serialized field is missing");
    }

    [Test]
    public void Process_WithoutSchema_Throw()
    {
        // Arrange
        var package = new Package
        {
            Serialized = _jsonPackage
        };

        // Act
        Action act = () => Middleware.Process(package);

        // Assert
        act.Should().Throw<FatNetLibException>()
            .WithMessage("Schema field is missing");
    }
}