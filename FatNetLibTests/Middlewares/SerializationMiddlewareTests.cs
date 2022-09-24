using System.Collections.Generic;
using FluentAssertions;
using Kolyhalov.FatNetLib;
using Kolyhalov.FatNetLib.Middlewares;
using NUnit.Framework;

namespace FatNetLibTests.Middlewares;

public class SerializationMiddlewareTests
{
    [Test]
    public void Process_SomePackage_ReturnJson()
    {
        // Arrange
        var package = new Package
        {
            Route = "some-route",
            Body = new Dictionary<string, object>
            {
                { "entityId", 123 }
            }
        };
        var middleware = new SerializationMiddleware();

        // Act
        middleware.Process(package);

        // Assert
        package.Serialized.Should().Be("{\"Route\":\"some-route\",\"Body\":{\"entityId\":123}}");
    }
}