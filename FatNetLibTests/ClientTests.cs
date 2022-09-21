using System;
using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using Kolyhalov.FatNetLib;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Middlewares;
using Kolyhalov.FatNetLib.NetPeers;
using Kolyhalov.FatNetLib.ResponsePackageMonitors;
using LiteNetLib;
using LiteNetLib.Utils;
using static Moq.Times;
using Moq;
using NUnit.Framework;

namespace FatNetLibTests;

public class ClientTests
{
    private EndpointsStorage _endpointsStorage = null!;
    private Mock<INetPeer> _netPeer = null!;
    private Mock<IResponsePackageMonitor> _responsePackageMonitor = null!;
    private Mock<IMiddlewaresRunner> _sendingMiddlewaresRunner = null!;
    private Mock<IMiddlewaresRunner> _receivingMiddlewaresRunner = null!;

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
        _sendingMiddlewaresRunner = APassingMiddlewareRunner();
        _receivingMiddlewaresRunner = APassingMiddlewareRunner();

        _connectedPeers = new List<INetPeer>();

        _client = new Client(_connectedPeers,
            _endpointsStorage,
            _responsePackageMonitor.Object,
            _sendingMiddlewaresRunner.Object,
            _receivingMiddlewaresRunner.Object);
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
        void Action() => _client.SendPackage(new Package { Route = "correct-route" }, It.IsAny<int>());

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
        void Action() => _client.SendPackage(new Package { Route = "correct-route" }, NetPeerId);

        // Assert 
        Assert.That(Action, Throws.TypeOf<FatNetLibException>().With.Message.Contains("Endpoint not found"));
    }

    [Test]
    public void SendPackage_ToExchangerWithoutExchangeId_GenerateExchangeId()
    {
        // Arrange
        var endpoint = new Endpoint("correct-route", EndpointType.Exchanger, DeliveryMethod.Sequenced);
        _endpointsStorage.RemoteEndpoints.Add(NetPeerId, new List<Endpoint> { endpoint });
        _connectedPeers.Add(_netPeer.Object);
        var requestPackage = new Package { Route = "correct-route", ExchangeId = null };
        _responsePackageMonitor.Setup(m => m.Wait(It.IsAny<Guid>()))
            .Returns(new Func<Guid, Package>(exchangeId => new Package { ExchangeId = exchangeId }));

        // Act
        Package? actualResponsePackage = _client.SendPackage(requestPackage, NetPeerId);

        // Assert
        actualResponsePackage!.ExchangeId.Should().NotBeNull();
    }

    [Test]
    public void SendPackage_ToReceivingPeer_SendAndReturnNull()
    {
        // Arrange
        var route = "correct-route";
        var deliveryMethod = DeliveryMethod.Sequenced;
        var endpoint = new Endpoint(route, EndpointType.Receiver, deliveryMethod);
        _endpointsStorage.RemoteEndpoints[NetPeerId] = new List<Endpoint> { endpoint };
        _connectedPeers.Add(_netPeer.Object);
        var package = new Package { Route = route };

        // Act
        Package? result = _client.SendPackage(package, NetPeerId);

        // Assert
        Assert.AreEqual(null, result);
        _netPeer.Verify(netPeer => netPeer.Send(It.IsAny<NetDataWriter>(), deliveryMethod));
    }

    [Test]
    public void SendPackage_ToReceivingPeer_SendingMiddlewareRunnerCalled()
    {
        // Arrange
        var endpoint = new Endpoint("correct-route", EndpointType.Receiver, DeliveryMethod.Sequenced);
        _endpointsStorage.RemoteEndpoints[NetPeerId] = new List<Endpoint> { endpoint };
        _connectedPeers.Add(_netPeer.Object);
        var package = new Package { Route = "correct-route" };

        // Act
        _client.SendPackage(package, NetPeerId);

        // Assert
        _sendingMiddlewaresRunner.Verify(runner => runner.Process(package), Once);
        _sendingMiddlewaresRunner.VerifyNoOtherCalls();
        _receivingMiddlewaresRunner.VerifyNoOtherCalls();
    }

    [Test]
    public void SendPackage_ThroughReplacingSendingMiddleware_ReplacedPackageUsed()
    {
        // todo: write this test when refactoring is done
    }

    [Test]
    public void SendPackage_ToExchanger_WaitAndReturnResponsePackage()
    {
        // Arrange
        var endpoint = new Endpoint("correct-route", EndpointType.Exchanger, DeliveryMethod.Sequenced);
        _endpointsStorage.RemoteEndpoints[NetPeerId] = new List<Endpoint> { endpoint };
        _connectedPeers.Add(_netPeer.Object);
        var requestPackage = new Package { Route = "correct-route", ExchangeId = Guid.NewGuid() };
        var expectedResponsePackage = new Package();
        _responsePackageMonitor.Setup(m => m.Wait(It.IsAny<Guid>()))
            .Returns(expectedResponsePackage);

        // Act
        Package? actualResponsePackage = _client.SendPackage(requestPackage, NetPeerId);

        // Assert
        actualResponsePackage.Should().Be(expectedResponsePackage);
        _netPeer.Verify(netPeer => netPeer.Send(It.IsAny<NetDataWriter>(), DeliveryMethod.Sequenced));
        _responsePackageMonitor.Verify(m => m.Wait(It.IsAny<Guid>()), Once);
        _responsePackageMonitor.Verify(m => m.Wait(
            It.Is<Guid>(exchangeId => exchangeId == requestPackage.ExchangeId)));
    }

    [Test]
    public void SendPackage_ToExchangingPeer_SendingAndReceivingMiddlewareRunnersCalled()
    {
        // Arrange
        var endpoint = new Endpoint("correct-route", EndpointType.Exchanger, DeliveryMethod.Sequenced);
        _endpointsStorage.RemoteEndpoints[NetPeerId] = new List<Endpoint> { endpoint };
        _connectedPeers.Add(_netPeer.Object);
        var requestPackage = new Package { Route = "correct-route", ExchangeId = Guid.NewGuid() };

        // Act
        Package? responsePackage = _client.SendPackage(requestPackage, NetPeerId);

        // Assert
        _sendingMiddlewaresRunner.Verify(_ => _.Process(requestPackage), Once);
        _sendingMiddlewaresRunner.VerifyNoOtherCalls();
        _receivingMiddlewaresRunner.Verify(_ => _.Process(responsePackage!), Once);
        _receivingMiddlewaresRunner.VerifyNoOtherCalls();
    }

    [Test]
    public void SendPackage_ThroughReplacingReceivingMiddleware_ReplacedPackageReturned()
    {
        // Arrange
        var endpoint = new Endpoint("correct-route", EndpointType.Exchanger, DeliveryMethod.Sequenced);
        _endpointsStorage.RemoteEndpoints[NetPeerId] = new List<Endpoint> { endpoint };
        _connectedPeers.Add(_netPeer.Object);
        var requestPackage = new Package { Route = "correct-route", ExchangeId = Guid.NewGuid() };

        var initialResponsePackage = new Package { ExchangeId = Guid.NewGuid() };
        _responsePackageMonitor.Setup(_ => _.Wait(It.IsAny<Guid>()))
            .Returns(initialResponsePackage);

        var replacedResponsePackage = new Package();
        _receivingMiddlewaresRunner.Setup(_ => _.Process(initialResponsePackage))
            .Returns(replacedResponsePackage);

        // Act
        Package? actualResponsePackage = _client.SendPackage(requestPackage, NetPeerId);

        // Assert
        actualResponsePackage.Should().Be(replacedResponsePackage);
    }

    private static Mock<IMiddlewaresRunner> APassingMiddlewareRunner()
    {
        var middlewareRunner = new Mock<IMiddlewaresRunner>();
        middlewareRunner.Setup(_ => _.Process(It.IsAny<Package>()))
            .Returns<Package>(package => package);
        return middlewareRunner;
    }
}