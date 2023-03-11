﻿using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Wrappers;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using static System.Text.Encoding;

namespace Kolyhalov.FatNetLib.Json.Tests.Middlewares
{
    public class JsonDeserializationMiddlewareTests
    {
        private readonly byte[] _jsonPackage = UTF8.GetBytes(@"{
        ""Route"": ""some-route"",
        ""Body"": ""some-body"",
        ""IsResponse"": ""false"",
    }");

        private readonly byte[] _jsonResponsePackage = UTF8.GetBytes(@"{
        ""Route"": ""some-route"",
        ""Body"": ""some-body"",
        ""IsResponse"": ""true"",
    }");

        private static readonly JsonSerializer JsonSerializer = JsonSerializer.Create(
            new JsonSerializerSettings
            {
                Converters = { new RouteConverter() }
            });

        private readonly PackageSchema _defaultPackageSchema = new()
        {
            { nameof(Package.Route), typeof(Route) },
            { nameof(Package.IsResponse), typeof(bool) },
            { nameof(Package.ExchangeId), typeof(Guid) }
        };

        private static readonly JsonDeserializationMiddleware Middleware = new(JsonSerializer);

        private DependencyContext _context = null!;

        private Mock<IEndpointsStorage> _endpointsStorage = null!;

        [SetUp]
        public void SetUp()
        {
            _endpointsStorage = new Mock<IEndpointsStorage>();
            _endpointsStorage.Setup(_ => _.LocalEndpoints)
                .Returns(SomeLocalEndpoints());
            _endpointsStorage.Setup(_ => _.RemoteEndpoints)
                .Returns(SomeEndpoints());
            _context = new DependencyContext();
            _context.Put(_endpointsStorage.Object);
        }

        [Test]
        public void Process_CorrectPackage_Pass()
        {
            // Arrange
            Package package = APackage();

            // Act
            Middleware.Process(package);

            // Assert
            package.Route.Should().Be(new Route("some-route"));
            package.Body.Should().Be("some-body");
        }

        [Test]
        public void Process_WithoutSerialized_Throw()
        {
            // Arrange
            Package package = APackage();
            package.RemoveNonSendingField(nameof(Package.Serialized));

            // Act
            Action act = () => Middleware.Process(package);

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage("Package must contain Serialized field");
        }

        [Test]
        public void Process_WithoutSchema_Throw()
        {
            // Arrange
            Package package = APackage();
            package.RemoveNonSendingField(nameof(Package.Schema));

            // Act
            Action act = () => Middleware.Process(package);

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage("Package must contain Schema field");
        }

        [Test]
        public void Process_WithoutContext_Throw()
        {
            // Arrange
            Package package = APackage();
            package.RemoveNonSendingField(nameof(Package.Context));

            // Act
            Action act = () => Middleware.Process(package);

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage("Package must contain Context field");
        }

        [Test]
        public void Process_WithoutRoute_Throw()
        {
            // Arrange
            Package package = APackage();
            package.Serialized = UTF8.GetBytes(@"{""Body"": ""some-body""}");

            // Act
            Action act = () => Middleware.Process(package);

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage("Route field is missing");
        }

        [Test]
        public void Process_CorrectResponsePackage_Pass()
        {
            // Arrange
            Package package = APackage();
            package.Serialized = _jsonResponsePackage;

            // Act
            Middleware.Process(package);

            // Assert
            package.Route.Should().Be(new Route("some-route"));
            package.Body.Should().Be("some-body");
        }

        private Package APackage()
        {
            return new Package
            {
                Serialized = _jsonPackage,
                Schema = new PackageSchema(_defaultPackageSchema),
                Context = _context,
                FromPeer = Mock.Of<INetPeer>()
            };
        }

        private static IList<LocalEndpoint> SomeLocalEndpoints()
        {
            return new List<LocalEndpoint>
            {
                new LocalEndpoint(
                    new Endpoint(
                        new Route("some-route"),
                        EndpointType.Receiver,
                        Reliability.Sequenced,
                        requestSchemaPatch: new PackageSchema { { nameof(Package.Body), typeof(string) } },
                        responseSchemaPatch: new PackageSchema { { nameof(Package.Body), typeof(string) } }),
                    action: new Action(() => { }))
            };
        }

        private static Dictionary<int, IList<Endpoint>> SomeEndpoints()
        {
            return new Dictionary<int, IList<Endpoint>>
            {
                { 0, SomeLocalEndpoints().Select(_ => _.Details).ToList() }
            };
        }
    }
}
