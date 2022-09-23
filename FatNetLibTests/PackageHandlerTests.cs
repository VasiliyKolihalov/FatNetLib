using System;
using System.Collections.Generic;
using AutoFixture;
using AutoFixture.NUnit3;
using FluentAssertions;
using Kolyhalov.FatNetLib;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Middlewares;
using Kolyhalov.FatNetLib.NetPeers;
using LiteNetLib;
using LiteNetLib.Utils;
using Moq;
using NUnit.Framework;
using static Moq.Times;


namespace FatNetLibTests;

public class PackageHandlerTests
{
    private IEndpointsStorage _endpointsStorage = null!;
    private Mock<IEndpointsInvoker> _endpointsInvoker = null!;
    private Mock<IMiddlewaresRunner> _sendingMiddlewaresRunner = null!;
    private Mock<IMiddlewaresRunner> _receivingMiddlewaresRunner = null!;
    private List<INetPeer> _connectedPeers = null!;
    private Mock<INetPeer> _netPeer = null!;
    private IPackageHandler _packageHandler = null!;

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
        _endpointsInvoker = new Mock<IEndpointsInvoker>();
        _sendingMiddlewaresRunner = APassingMiddlewareRunner();
        _receivingMiddlewaresRunner = APassingMiddlewareRunner();
        _connectedPeers = new List<INetPeer>();
        _packageHandler = new PackageHandler(_endpointsStorage,
            _endpointsInvoker.Object,
            _sendingMiddlewaresRunner.Object,
            _receivingMiddlewaresRunner.Object,
            _connectedPeers);
    }

    [Test, AutoData]
    public void Handle_Receiver_Invoke(DeliveryMethod deliveryMethod, string route)
    {
        // Arrange
        var endpoint = new Endpoint(route, EndpointType.Receiver, deliveryMethod);
        var localEndpoint = new LocalEndpoint(endpoint, new Fixture().Create<ReceiverDelegate>());
        _endpointsStorage.LocalEndpoints.Add(localEndpoint);
        _connectedPeers.Add(_netPeer.Object);

        var package = new Package
        {
            Route = route
        };

        //Act
        _packageHandler.Handle(package, NetPeerId, deliveryMethod);

        //Assert
        _endpointsInvoker.Verify(invoker => invoker.InvokeReceiver(localEndpoint, package));
        _sendingMiddlewaresRunner.Verify(runner => runner.Process(package), Once);
        _sendingMiddlewaresRunner.VerifyNoOtherCalls();
        _receivingMiddlewaresRunner.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public void Handle_Exchanger_InvokeAndSendResponse(Package responsePackage, DeliveryMethod deliveryMethod)
    {
        // Arrange
        _endpointsInvoker.Setup(x => x.InvokeExchanger(It.IsAny<LocalEndpoint>(), It.IsAny<Package>()))
            .Returns(responsePackage);
        var route = "some-path";
        var endpoint = new Endpoint(route, EndpointType.Exchanger, deliveryMethod);
        var localEndpoint = new LocalEndpoint(endpoint, new Fixture().Create<ReceiverDelegate>());
        _endpointsStorage.LocalEndpoints.Add(localEndpoint);
        _connectedPeers.Add(_netPeer.Object);

        var package = new Package
        {
            Route = route
        };

        //Act
        _packageHandler.Handle(package, NetPeerId, deliveryMethod);

        //Assert
        responsePackage.Route.Should().Be(route);
        responsePackage.ExchangeId.Should().Be(package.ExchangeId);
        responsePackage.IsResponse.Should().Be(true);
        
        _endpointsInvoker.Verify(invoker => invoker.InvokeExchanger(localEndpoint, package));
        _sendingMiddlewaresRunner.Verify(runner => runner.Process(package), Once);
        _sendingMiddlewaresRunner.VerifyNoOtherCalls();
        _receivingMiddlewaresRunner.Verify(runner => runner.Process(responsePackage), Once);
        _receivingMiddlewaresRunner.VerifyNoOtherCalls();
        _netPeer.Verify(netPeer => netPeer.Send(It.IsAny<NetDataWriter>(), deliveryMethod));
    }

    [Test]
    public void Handle_NonExistentEndpoint_Throw()
    {
        // Arrange
        var package = new Package
        {
            Route = "some-route"
        };

        //Act
        Action action = () => _packageHandler.Handle(package, It.IsAny<int>(), It.IsAny<DeliveryMethod>());

        //Assert
        action.Should().Throw<FatNetLibException>()
            .WithMessage("Package from 0 pointed to a non-existent endpoint. Route: some-route");
    }

    [Test, AutoData]
    public void Handle_WrongDeliveryMethod_Throw(DeliveryMethod deliveryMethod, string route)
    {
        // Arrange
        var endpoint = new Endpoint(route, EndpointType.Exchanger, deliveryMethod);
        var localEndpoint = new LocalEndpoint(endpoint, new Fixture().Create<ReceiverDelegate>());
        _endpointsStorage.LocalEndpoints.Add(localEndpoint);

        var package = new Package
        {
            Route = route
        };

        //Act
        Action action = () => _packageHandler.Handle(package, NetPeerId, DeliveryMethod.Unreliable);

        //Assert
        action.Should().Throw<FatNetLibException>().WithMessage($"Package from {NetPeerId} came with the wrong type of delivery");
    }

    private static Mock<IMiddlewaresRunner> APassingMiddlewareRunner()
    {
        var middlewareRunner = new Mock<IMiddlewaresRunner>();
        middlewareRunner.Setup(_ => _.Process(It.IsAny<Package>()))
            .Returns<Package>(package => package);
        return middlewareRunner;
    }
}