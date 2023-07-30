using System.Diagnostics;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Middlewares;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Modules.Defaults.Client;
using Kolyhalov.FatNetLib.Core.Modules.Defaults.Server;
using Kolyhalov.FatNetLib.Json;
using NUnit.Framework;
using static Kolyhalov.FatNetLib.Core.Constants.RouteConstants.Routes.Events;
using static Kolyhalov.FatNetLib.IntegrationTests.TestUtils;


namespace FatNetLib.PerformanceTests;

public class AsyncTests
{
    private Kolyhalov.FatNetLib.Core.FatNetLib _clientFatNetLib = null!;
    private readonly ManualResetEventSlim _serverReadyEvent = new();
    private readonly ManualResetEventSlim _clientReadyEvent = new();
    private static readonly Route EndpointRoute = new("async-endpoint");
    private int _countOfInvokes;
    private static readonly TimeSpan EndpointDelay = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan Overheads = TimeSpan.FromSeconds(1);

    [SetUp]
    public void Setup()
    {
        _countOfInvokes = 0;
        Port port = FindFreeTcpPort();
        RunServerFatNetLib(port);
        _clientFatNetLib = RunClientFatNetLib(port);
        _serverReadyEvent.Wait(TimeSpan.FromSeconds(5)).Should().BeTrue();
        _clientReadyEvent.Wait(TimeSpan.FromSeconds(5)).Should().BeTrue();
    }

    private void RunServerFatNetLib(Port port)
    {
        var fatNetLib = new FatNetLibBuilder
        {
            Modules =
            {
                new DefaultServerModule(),
                new JsonModule()
            },
            SendingMiddlewaresOrder = new[]
            {
                typeof(JsonSerializationMiddleware),
                typeof(EncryptionMiddleware)
            },
            ReceivingMiddlewaresOrder = new[]
            {
                typeof(DecryptionMiddleware),
                typeof(JsonDeserializationMiddleware)
            },
            ConfigurationPatch = new ServerConfiguration { Port = port }
        };

        fatNetLib.Endpoints.AddEventListener(InitializationFinished, _ => _serverReadyEvent.Set());
        fatNetLib.Endpoints.AddExchanger(
            route: EndpointRoute,
            action: async () =>
            {
                Interlocked.Increment(ref _countOfInvokes);
                await Task.Delay(EndpointDelay);
                return new Package();
            });

        fatNetLib.BuildAndRun();
    }

    private Kolyhalov.FatNetLib.Core.FatNetLib RunClientFatNetLib(Port port)
    {
        var fatNetLib = new FatNetLibBuilder
        {
            Modules =
            {
                new DefaultClientModule(),
                new JsonModule()
            },
            SendingMiddlewaresOrder = new[]
            {
                typeof(JsonSerializationMiddleware),
                typeof(EncryptionMiddleware)
            },
            ReceivingMiddlewaresOrder = new[]
            {
                typeof(DecryptionMiddleware),
                typeof(JsonDeserializationMiddleware)
            },
            ConfigurationPatch = new ClientConfiguration { Port = port }
        };

        fatNetLib.Endpoints.AddEventListener(InitializationFinished, _ => _clientReadyEvent.Set());

        return fatNetLib.BuildAndRun();
    }

    /// <summary>
    /// Verifies that the asynchronous courier and asynchronous endpoint
    /// are not blocking a thread while waiting for an asynchronous load.
    /// The thread must be returned to the thread pool and be ready to process other requests.
    /// If this mechanism fails, this test will send enough concurrent requests,
    /// to get significant degradation in response time and test failure.
    /// </summary>
    [Test]
    public void Test()
    {
        // Arrange
        int cpuThreads = Environment.ProcessorCount;
        var tasks = new List<Task>();
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (var i = 0; i < cpuThreads * 20; i++)
        {
            tasks.Add(_clientFatNetLib.ClientCourier!.SendToServerAsync(new Package
            {
                Route = EndpointRoute
            }));
        }

        Task.WaitAll(tasks.ToArray());
        stopwatch.Stop();

        // Assert
        _countOfInvokes.Should().Be(tasks.Count);
        stopwatch.ElapsedMilliseconds.Should()
            .BeLessOrEqualTo((long)(EndpointDelay.TotalMilliseconds + Overheads.TotalMilliseconds));
    }
}
