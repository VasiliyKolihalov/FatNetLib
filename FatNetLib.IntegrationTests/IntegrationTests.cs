using System;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Threading;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core;
using Kolyhalov.FatNetLib.Core.Attributes;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Controllers;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Middlewares;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Modules.Defaults;
using Kolyhalov.FatNetLib.Core.Modules.Defaults.Client;
using Kolyhalov.FatNetLib.Core.Modules.Defaults.Server;
using Kolyhalov.FatNetLib.Json;
using Kolyhalov.FatNetLib.MicrosoftLogging;
using NUnit.Framework;
using static Kolyhalov.FatNetLib.Core.Constants.RouteConstants.Routes.Events;
using static Kolyhalov.FatNetLib.IntegrationTests.TestUtils;

namespace Kolyhalov.FatNetLib.IntegrationTests;

[Timeout(10000)] // 10 seconds
public class IntegrationTests
{
    private readonly ManualResetEventSlim _serverReadyEvent = new();
    private readonly ManualResetEventSlim _clientReadyEvent = new();
    private readonly ManualResetEventSlim _consumerCallEvent = new();
    private readonly ReferenceContainer<Package> _consumerCallEventPackage = new();
    private readonly ReferenceContainer<Package> _exchangerCallEventPackage = new();
    private Core.FatNetLib _serverFatNetLib = null!;
    private Core.FatNetLib _clientFatNetLib = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Port port = FindFreeTcpPort();
        _serverFatNetLib = RunServerFatNetLib(port);
        _clientFatNetLib = RunClientFatNetLib(port);
        _serverReadyEvent.Wait(TimeSpan.FromSeconds(5)).Should().BeTrue();
        _clientReadyEvent.Wait(TimeSpan.FromSeconds(5)).Should().BeTrue();
    }

    [SetUp]
    public void SetUp()
    {
        _serverReadyEvent.Reset();
        _clientReadyEvent.Reset();
        _consumerCallEvent.Reset();
        _consumerCallEventPackage.Clear();
        _exchangerCallEventPackage.Clear();
    }

    [Test]
    public void SendPackageToControllerStyleConsumer()
    {
        // Act
        _clientFatNetLib.Courier.Send(new Package
        {
            Route = new Route("test/consumer/call"),
            Body = new TestBody { Data = "test-data" },
            Receiver = _clientFatNetLib.ClientCourier!.ServerPeer
        });

        // Assert
        _consumerCallEvent.Wait();
        _consumerCallEventPackage.Reference.Body.Should().BeEquivalentTo(new TestBody { Data = "test-data" });
    }

    [Test]
    public void SendPackageToBuilderStyleExchanger()
    {
        // Act
        Package responsePackage = _serverFatNetLib.Courier.Send(new Package
        {
            Route = new Route("test/exchanger/call"),
            Body = new TestBody { Data = "test-request" },
            Receiver = _serverFatNetLib.Courier.Peers[0]
        })!;

        // Assert
        _exchangerCallEventPackage.Reference.Body.Should().BeEquivalentTo(new TestBody { Data = "test-request" });
        responsePackage.Body.Should().BeEquivalentTo(new TestBody { Data = "test-response" });
    }

    private Core.FatNetLib RunServerFatNetLib(Port port)
    {
        var fatNetLib = new FatNetLibBuilder
        {
            Modules =
            {
                new MicrosoftLoggerModule(For.Server),
                new DefaultServerModule(),
                new JsonModule(),
                new CompressionModule(CompressionLevel.Optimal),
            },
            SendingMiddlewaresOrder = new[]
            {
                typeof(JsonSerializationMiddleware),
                typeof(CompressionMiddleware),
                typeof(EncryptionMiddleware)
            },
            ReceivingMiddlewaresOrder = new[]
            {
                typeof(DecryptionMiddleware),
                typeof(DecompressionMiddleware),
                typeof(JsonDeserializationMiddleware)
            },
            ConfigurationPatch = new ServerConfiguration { Port = port }
        };

        fatNetLib.Endpoints.AddEvent(InitializationFinished, _ => _serverReadyEvent.Set());
        fatNetLib.Endpoints.AddController(new TestController(
            _consumerCallEvent,
            _consumerCallEventPackage));

        return fatNetLib.BuildAndRun();
    }

    private Core.FatNetLib RunClientFatNetLib(Port port)
    {
        var fatNetLib = new FatNetLibBuilder
        {
            Modules =
            {
                new MicrosoftLoggerModule(For.Client),
                new DefaultClientModule(),
                new JsonModule(),
                new CompressionModule(CompressionLevel.Optimal)
            },
            SendingMiddlewaresOrder = new[]
            {
                typeof(JsonSerializationMiddleware),
                typeof(CompressionMiddleware),
                typeof(EncryptionMiddleware)
            },
            ReceivingMiddlewaresOrder = new[]
            {
                typeof(DecryptionMiddleware),
                typeof(DecompressionMiddleware),
                typeof(JsonDeserializationMiddleware)
            },
            ConfigurationPatch = new ClientConfiguration { Port = port }
        };

        fatNetLib.Endpoints.AddEvent(InitializationFinished, _ => _clientReadyEvent.Set());
        fatNetLib.Endpoints.AddExchanger(
            new Route("test/exchanger/call"),
            package =>
            {
                _exchangerCallEventPackage.Reference = package;
                return new Package
                {
                    Body = new TestBody { Data = "test-response" }
                };
            },
            requestSchemaPatch: new PackageSchema { { nameof(Package.Body), typeof(TestBody) } },
            responseSchemaPatch: new PackageSchema { { nameof(Package.Body), typeof(TestBody) } });

        return fatNetLib.BuildAndRun();
    }
}

[SuppressMessage("ReSharper", "UnusedMember.Global")]
internal class TestController : IController
{
    private readonly ManualResetEventSlim _consumerCallEvent;
    private readonly ReferenceContainer<Package> _consumerCallPackage;

    public TestController(ManualResetEventSlim consumerCallEvent, ReferenceContainer<Package> consumerCallPackage)
    {
        _consumerCallEvent = consumerCallEvent;
        _consumerCallPackage = consumerCallPackage;
    }

    [Consumer]
    [Route("test/consumer/call")]
    [Schema(key: nameof(Package.Body), type: typeof(TestBody))]
    public void CallTestConsumer(Package package)
    {
        _consumerCallPackage.Reference = package;
        _consumerCallEvent.Set();
    }
}

internal class ReferenceContainer<T>
{
    public T Reference { get; set; } = default!;

    public void Clear()
    {
        Reference = default!;
    }
}

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
internal class TestBody
{
    public string Data { get; set; } = null!;
}
