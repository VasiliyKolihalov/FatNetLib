using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using FluentAssertions;
using Kolyhalov.FatNetLib.Attributes;
using Kolyhalov.FatNetLib.Microtypes;
using Kolyhalov.FatNetLib.Modules.DefaultModules;
using LiteNetLib;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.IntegrationTests;

[Timeout(10000)]
public class IntegrationTests
{
    private FatNetLib _serverFatNetLib = null!;
    private FatNetLib _clientFatNetLib = null!;
    private readonly ManualResetEventSlim _serverReadyEvent = new();
    private readonly ManualResetEventSlim _clientReadyEvent = new();
    private readonly ManualResetEventSlim _receiverCallEvent = new();
    private readonly ReferenceContainer<Package> _receiverCallEventPackage = new();
    private readonly ReferenceContainer<Package> _exchangerCallEventPackage = new();

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _serverFatNetLib = CreateServerFatNetLib();
        _clientFatNetLib = CreateClientFatNetLib();
        _serverFatNetLib.Run();
        _clientFatNetLib.Run();
        _serverReadyEvent.Wait(TimeSpan.FromSeconds(5)).Should().BeTrue();
        _clientReadyEvent.Wait(TimeSpan.FromSeconds(5)).Should().BeTrue();
    }

    [SetUp]
    public void SetUp()
    {
        _serverReadyEvent.Reset();
        _clientReadyEvent.Reset();
        _receiverCallEvent.Reset();
        _receiverCallEventPackage.Clear();
        _exchangerCallEventPackage.Clear();
    }

    [Test]
    public void SendPackageToControllerStyleReceiver()
    {
        // Act
        _clientFatNetLib.Client.SendPackage(new Package
        {
            Route = new Route("test/receiver/call"),
            Body = new TestBody { Data = "test-data" },
            ToPeerId = 0
        });

        // Assert
        _receiverCallEvent.Wait();
        _receiverCallEventPackage.Reference!.Body.Should().BeEquivalentTo(new TestBody { Data = "test-data" });
    }

    [Test]
    public void SendPackageToBuilderStyleExchanger()
    {
        // Act
        Package responsePackage = _serverFatNetLib.Client.SendPackage(new Package
        {
            Route = new Route("test/exchanger/call"),
            Body = new TestBody { Data = "test-request" },
            ToPeerId = 0
        })!;

        // Assert
        _exchangerCallEventPackage.Reference!.Body.Should().BeEquivalentTo(new TestBody { Data = "test-request" });
        responsePackage.Body.Should().BeEquivalentTo(new TestBody { Data = "test-response" });
    }

    private FatNetLib CreateServerFatNetLib()
    {
        var builder = new FatNetLibBuilder();
        builder.Modules.Register(new ServerModule());
        builder.Endpoints.AddController(new TestController(_receiverCallEvent,
            _receiverCallEventPackage));

        FatNetLib fatNetLib = builder.Build();
        builder.Endpoints.AddExchanger("fat-net-lib/finish-initialization",
            DeliveryMethod.ReliableOrdered,
            exchangerDelegate: package =>
            {
                _serverReadyEvent.Set();
                package.Client!.SendPackage(new Package
                {
                    Route = new Route("fat-net-lib/finish-initialization"),
                    ToPeerId = 0
                });
                return new Package();
            },
            isInitial: true
        );
        return fatNetLib;
    }

    private FatNetLib CreateClientFatNetLib()
    {
        var builder = new FatNetLibBuilder();
        builder.Modules.Register(new ClientModule());
        FatNetLib fatNetLib = builder.Build();
        builder.Endpoints.AddExchanger("fat-net-lib/finish-initialization",
            DeliveryMethod.ReliableOrdered,
            exchangerDelegate: _ =>
            {
                _clientReadyEvent.Set();
                return new Package();
            },
            isInitial: true
        );

        builder.Endpoints.AddExchanger(
            "test/exchanger/call",
            DeliveryMethod.ReliableSequenced,
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

        return fatNetLib;
    }
}

[SuppressMessage("ReSharper", "UnusedMember.Global")]
internal class TestController : IController
{
    private readonly ManualResetEventSlim _receiverCallEvent;
    private readonly ReferenceContainer<Package> _receiverCallPackage;

    public TestController(ManualResetEventSlim receiverCallEvent, ReferenceContainer<Package> receiverCallPackage)
    {
        _receiverCallEvent = receiverCallEvent;
        _receiverCallPackage = receiverCallPackage;
    }

    [Receiver]
    [Route("test/receiver/call")]
    [Schema(key: nameof(Package.Body), type: typeof(TestBody))]
    public void RunTestReceiver(Package package)
    {
        _receiverCallPackage.Reference = package;
        _receiverCallEvent.Set();
    }
}

internal class ReferenceContainer<T>
{
    public T? Reference { get; set; }

    public void Clear()
    {
        Reference = default;
    }
}

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
internal class TestBody
{
    public string Data { get; set; } = null!;
}