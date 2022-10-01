using System.Collections.Generic;
using FluentAssertions;
using Kolyhalov.FatNetLib;
using Kolyhalov.FatNetLib.Middlewares;
using Newtonsoft.Json;
using NUnit.Framework;

namespace FatNetLibTests.Middlewares;

public class SerializationMiddlewareTests
{
    
    private static readonly JsonSerializer JsonSerializer = JsonSerializer.Create(
        new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> { new RouteConverter() }
        });

    private static readonly SerializationMiddleware Middleware = new (JsonSerializer);
    

    [Test]
    public void Process_SomePackage_ReturnJson()
    {
        // Arrange
        var package = new Package
        {
            Route = new Route("some-route"),
            Body = new Dictionary<string, object>
            {
                { "entityId", 123 }
            }
        };

        // Act
        Middleware.Process(package);

        // Assert
        package.Serialized.Should().Be("{\"Route\":\"some-route\",\"Body\":{\"entityId\":123}}");
    }
}