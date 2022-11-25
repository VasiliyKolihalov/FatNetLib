using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core;
using Kolyhalov.FatNetLib.Core.Attributes;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Controllers;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Models;
using NUnit.Framework;
using static Kolyhalov.FatNetLib.Core.Controllers.RouteConstants.Routes.Events;

namespace Kolyhalov.FatNetLib.IntegrationTests
{
    [Timeout(10000)] // 10 seconds
    public class IntegrationTests
    {
        private readonly ManualResetEventSlim _serverReadyEvent = new ManualResetEventSlim();
        private readonly ManualResetEventSlim _clientReadyEvent = new ManualResetEventSlim();
        private readonly ManualResetEventSlim _receiverCallEvent = new ManualResetEventSlim();
        private readonly ReferenceContainer<Package> _receiverCallEventPackage = new ReferenceContainer<Package>();
        private readonly ReferenceContainer<Package> _exchangerCallEventPackage = new ReferenceContainer<Package>();
        private Core.FatNetLib _serverFatNetLib = null!;
        private Core.FatNetLib _clientFatNetLib = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _serverFatNetLib = RunServerFatNetLib();
            _clientFatNetLib = RunClientFatNetLib();
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
            _clientFatNetLib.Courier.Send(new Package
            {
                Route = new Route("test/receiver/call"),
                Body = new TestBody { Data = "test-data" },
                ToPeerId = 0
            });

            // Assert
            _receiverCallEvent.Wait();
            _receiverCallEventPackage.Reference.Body.Should().BeEquivalentTo(new TestBody { Data = "test-data" });
        }

        [Test]
        public void SendPackageToBuilderStyleExchanger()
        {
            // Act
            Package responsePackage = _serverFatNetLib.Courier.Send(new Package
            {
                Route = new Route("test/exchanger/call"),
                Body = new TestBody { Data = "test-request" },
                ToPeerId = 0
            })!;

            // Assert
            _exchangerCallEventPackage.Reference.Body.Should().BeEquivalentTo(new TestBody { Data = "test-request" });
            responsePackage.Body.Should().BeEquivalentTo(new TestBody { Data = "test-response" });
        }

        private Core.FatNetLib RunServerFatNetLib()
        {
            var builder = new FatNetLibBuilder { Modules = { new TestServerModule() } };

            builder.Endpoints.AddController(new TestController(
                _receiverCallEvent,
                _receiverCallEventPackage));

            Core.FatNetLib fatNetLib = builder.BuildAndRun();

            builder.Endpoints.AddEvent(InitializationFinished, _ => _serverReadyEvent.Set());

            return fatNetLib;
        }

        private Core.FatNetLib RunClientFatNetLib()
        {
            var builder = new FatNetLibBuilder { Modules = { new TestClientModule() } };

            Core.FatNetLib fatNetLib = builder.BuildAndRun();

            builder.Endpoints.AddEvent(InitializationFinished, _ => _clientReadyEvent.Set());

            builder.Endpoints.AddExchanger(
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
}
