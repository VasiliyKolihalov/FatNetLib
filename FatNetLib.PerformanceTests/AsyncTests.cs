using System.Diagnostics;
using System.IO.Compression;
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
    private const int EndpointDelayMs = 1000;
    private const int OverheadsMs = 1000;


    [SetUp]
    public void Setup()
    {
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
            action: async () => { await Task.Delay(EndpointDelayMs); });

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
        int threadsCount = Process.GetCurrentProcess().Threads.Count;
        var tasks = new List<Task>();
        var stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < threadsCount * 20; i++)
        {
            tasks.Add(_clientFatNetLib.ClientCourier!.SendToServerAsync(new Package
            {
                Route = EndpointRoute
            }));
        }

        Task.WaitAll(tasks.ToArray());
        stopwatch.Stop();

        stopwatch.ElapsedMilliseconds.Should().BeLessOrEqualTo(EndpointDelayMs + OverheadsMs);
    }
}
