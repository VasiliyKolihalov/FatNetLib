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

/// <summary>
/// This test verifies the work of the asynchronous courier and endpoints invoker.
/// The client asynchronously sends packages to the server endpoint,
/// which simply hangs asynchronously on EndpointDelayMs.
/// The number of requests is equal to the number of threads multiplied by 20.
/// This check that the asynchrony is working and returns the waiting threads.
/// OverheadsMs is added to the total time. This is the overhead of calling asynchronous methods.
/// </summary>
public class AsyncTests
{
    private Kolyhalov.FatNetLib.Core.FatNetLib _clientFatNetLib = null!;
    private readonly ManualResetEventSlim _serverReadyEvent = new();
    private readonly ManualResetEventSlim _clientReadyEvent = new();
    private static readonly Route EndpointRoute = new("async-endpoint");
    private int _countOfInvokes;
    private static readonly TimeSpan EndpointDelayMs = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan OverheadsMs = TimeSpan.FromSeconds(1);


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
                _countOfInvokes++;
                await Task.Delay(EndpointDelayMs);
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

    [Test]
    public void Test()
    {
        // Arrange
        int threadsCount = Process.GetCurrentProcess().Threads.Count;
        var tasks = new List<Task>();
        var stopwatch = Stopwatch.StartNew();
        
        // Act
        for (var i = 0; i < threadsCount * 20; i++)
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
            .BeLessOrEqualTo((long)(EndpointDelayMs.TotalMilliseconds + OverheadsMs.TotalMilliseconds));
    }
}
