using System.Collections.Generic;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Tests.Utils;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Json.Tests.Middlewares
{
    public class JsonSerializationMiddlewareTests
    {
        private static readonly JsonSerializer JsonSerializer = JsonSerializer.Create(
            new JsonSerializerSettings
            {
                Converters = { new RouteConverter() }
            });

        private static readonly JsonSerializationMiddleware Middleware
            = new JsonSerializationMiddleware(JsonSerializer);

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
            package.Serialized.Should().BeEquivalentToUtf8(
                "{\"Route\":\"some-route\",\"Body\":{\"entityId\":123}}");
        }
    }
}
