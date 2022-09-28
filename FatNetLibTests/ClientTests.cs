using System;
using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using Kolyhalov.FatNetLib;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.LiteNetLibWrappers;
using Kolyhalov.FatNetLib.Middlewares;
using Kolyhalov.FatNetLib.ResponsePackageMonitors;
using LiteNetLib;
using Moq;
using NUnit.Framework;
using static Moq.Times;

namespace FatNetLibTests;

public class ClientTests
{
    private EndpointsStorage _endpointsStorage = null!;
    private Mock<INetPeer> _netPeer = null!;
    private Mock<IResponsePackageMonitor> _responsePackageMonitor = null!;
    private Mock<IMiddlewaresRunner> _sendingMiddlewaresRunner = null!;

    private List<INetPeer> _connectedPeers = null!;
    private IClient _client = null!;


    private int NetPeerId => _netPeer.Object.Id;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _netPeer = new Mock<INetPeer>();
        _netPeer.Setup(netPeer => netPeer.Id)
            .Returns(new Fixture().Create<int>());
    }

    [SetUp]
    public void SetUp()
    {
        _endpointsStorage = new EndpointsStorage();
        _responsePackageMonitor = new Mock<IResponsePackageMonitor>();
        _sendingMiddlewaresRunner = AMiddlewareRunner();

        _connectedPeers = new List<INetPeer>();

        _client = new Client(_connectedPeers,
            _endpointsStorage,
            _responsePackageMonitor.Object,
            _sendingMiddlewaresRunner.Object);
    }

    [Test]
    public void SendPackage_NullPackage_Throw()
    {
        // Act
        void Action() => _client.SendPackage(package: null!, It.IsAny<int>());

        // Assert 
        Assert.That(Action,
            Throws.TypeOf<ArgumentNullException>().With.Message
                .Contains("Value cannot be null. (Parameter 'package')"));
    }

    [Test]
    public void SendPackage_NotFoundReceivingPeer_Throw()
    {
        // Act
        void Action() => _client.SendPackage(new Package { Route = new Route("correct-route") }, It.IsAny<int>());

        // Assert 
        Assert.That(Action, Throws.TypeOf<FatNetLibException>().With.Message.Contains("Receiving peer not found"));
    }

    [Test]
    public void SendPackage_NotFoundEndpoint_Throw()
    {
        // Arrange
        _endpointsStorage.RemoteEndpoints[NetPeerId] = new List<Endpoint>();
        _connectedPeers.Add(_netPeer.Object);

        // Act
        void Action() => _client.SendPackage(new Package { Route = new Route("correct-route") }, NetPeerId);

        // Assert 
        Assert.That(Action, Throws.TypeOf<FatNetLibException>().With.Message.Contains("Endpoint not found"));
    }

    [Test]
    public void SendPackage_ToExchangerWithoutExchangeId_GenerateExchangeId()
    {
        // Arrange
        RegisterEndpoint();
        var requestPackage = new Package { Route = new Route("correct-route"), ExchangeId = Guid.Empty };
        _responsePackageMonitor.Setup(m => m.Wait(It.IsAny<Guid>()))
            .Returns(new Func<Guid, Package>(exchangeId => new Package { ExchangeId = exchangeId }));

        // Act
        Package? actualResponsePackage = _client.SendPackage(requestPackage, NetPeerId);

        // Assert
        actualResponsePackage!.ExchangeId.Should().NotBeEmpty();
    }

    [Test]
    public void SendPackage_ToReceivingPeer_SendAndReturnNull()
    {
        // Arrange
        RegisterEndpoint();
        var package = new Package { Route = new Route("correct-route") };

        // Act
        Package? result = _client.SendPackage(package, NetPeerId);

        // Assert
        Assert.AreEqual(null, result);
        _netPeer.Verify(netPeer => netPeer.Send("serialized-package", DeliveryMethod.Sequenced));
    }

    [Test]
    public void SendPackage_ToReceivingPeer_SendingMiddlewareRunnerCalled()
    {
        // Arrange
        RegisterEndpoint();
        var package = new Package { Route = new Route("correct-route") };

        // Act
        _client.SendPackage(package, NetPeerId);

        // Assert
        _sendingMiddlewaresRunner.Verify(runner => runner.Process(package), Once);
        _sendingMiddlewaresRunner.VerifyNoOtherCalls();
    }

    [Test]
    public void SendPackage_ToExchanger_WaitAndReturnResponsePackage()
    {
        // Arrange
        RegisterEndpoint();
        var requestPackage = new Package { Route = new Route("correct-route"), ExchangeId = Guid.NewGuid() };
        var expectedResponsePackage = new Package();
        _responsePackageMonitor.Setup(m => m.Wait(It.IsAny<Guid>()))
            .Returns(expectedResponsePackage);

        // Act
        Package? actualResponsePackage = _client.SendPackage(requestPackage, NetPeerId);

        // Assert
        actualResponsePackage.Should().Be(expectedResponsePackage);
        _netPeer.Verify(netPeer => netPeer.Send("serialized-package", DeliveryMethod.Sequenced));
        _responsePackageMonitor.Verify(m => m.Wait(It.IsAny<Guid>()), Once);
        _responsePackageMonitor.Verify(m => m.Wait(
            It.Is<Guid>(exchangeId => exchangeId == requestPackage.ExchangeId)));
    }

    [Test]
    public void SendPackage_ToExchangingPeer_SendingMiddlewareRunnerCalled()
    {
        // Arrange
        RegisterEndpoint();
        var requestPackage = new Package { Route = new Route("correct-route"), ExchangeId = Guid.NewGuid() };

        // Act
        _client.SendPackage(requestPackage, NetPeerId);

        // Assert
        _sendingMiddlewaresRunner.Verify(_ => _.Process(requestPackage), Once);
        _sendingMiddlewaresRunner.VerifyNoOtherCalls();
    }

    [Test]
    public void SendPackage_PackageWasNotSerializedByMiddlewares_Throw()
    {
        // Arrange
        RegisterEndpoint();
        var requestPackage = new Package { Route = new Route("correct-route"), ExchangeId = Guid.NewGuid() };
        _sendingMiddlewaresRunner.Setup(_ => _.Process(It.IsAny<Package>()))
            .Callback<Package>(package => { package.Serialized = null; });

        // Act
        Action act = () => _client.SendPackage(requestPackage, NetPeerId);

        // Assert
        act.Should().Throw<FatNetLibException>().WithMessage("Serialized field is missing");
    }

    private static Mock<IMiddlewaresRunner> AMiddlewareRunner()
    {
        var middlewareRunner = new Mock<IMiddlewaresRunner>();
        middlewareRunner.Setup(_ => _.Process(It.IsAny<Package>()))
            .Callback<Package>(package => { package.Serialized = "serialized-package"; });
        return middlewareRunner;
    }

    private void RegisterEndpoint()
    {
        var endpoint = new Endpoint(new Route("correct-route"), EndpointType.Exchanger, DeliveryMethod.Sequenced);
        _endpointsStorage.RemoteEndpoints[NetPeerId] = new List<Endpoint> { endpoint };
        _connectedPeers.Add(_netPeer.Object);
    }
}