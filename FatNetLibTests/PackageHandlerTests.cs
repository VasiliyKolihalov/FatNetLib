using System;
using System.Collections.Generic;
using AutoFixture;
using AutoFixture.NUnit3;
using FluentAssertions;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Microtypes;
using Kolyhalov.FatNetLib.Middlewares;
using Kolyhalov.FatNetLib.Wrappers;
using LiteNetLib;
using Moq;
using NUnit.Framework;
using static System.Text.Encoding;
using static Moq.Times;


namespace Kolyhalov.FatNetLib;

public class PackageHandlerTests
{
    private IEndpointsStorage _endpointsStorage = null!;
    private Mock<IEndpointsInvoker> _endpointsInvoker = null!;
    private Mock<IMiddlewaresRunner> _sendingMiddlewaresRunner = null!;
    private List<INetPeer> _connectedPeers = null!;
    private Mock<INetPeer> _netPeer = null!;
    private IPackageHandler _packageHandler = null!;
    private readonly Mock<DependencyContext> _context = new();

    private int PeerId => _netPeer.Object.Id;

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
        _sendingMiddlewaresRunner = new Mock<IMiddlewaresRunner>();
        _connectedPeers = new List<INetPeer>();
        _packageHandler = new PackageHandler(_endpointsStorage,
            _endpointsInvoker.Object,
            _sendingMiddlewaresRunner.Object,
            _connectedPeers,
            _context.Object);
    }

    [Test, AutoData]
    public void Handle_Receiver_Invoke(DeliveryMethod deliveryMethod, string route)
    {
        // Arrange
        var endpoint = new Endpoint(new Route(route), EndpointType.Receiver, deliveryMethod);
        var localEndpoint = new LocalEndpoint(endpoint, new Fixture().Create<ReceiverDelegate>());
        _endpointsStorage.LocalEndpoints.Add(localEndpoint);
        _connectedPeers.Add(_netPeer.Object);

        var package = new Package
        {
            Route = new Route(route),
            ToPeerId = PeerId,
            DeliveryMethod = deliveryMethod
        };

        //Act
        _packageHandler.Handle(package);

        //Assert
        _endpointsInvoker.Verify(_ => _.InvokeReceiver(localEndpoint, package));
        _sendingMiddlewaresRunner.VerifyNoOtherCalls();
    }

    [Test, AutoData]
    public void Handle_Exchanger_InvokeAndSendResponse(DeliveryMethod deliveryMethod)
    {
        // Arrange
        var requestPackage = new Package { FromPeerId = PeerId, DeliveryMethod = deliveryMethod };
        var responsePackage = new Package();
        requestPackage.Route = new Route("some-route");
        var endpoint = new Endpoint(new Route("some-route"), EndpointType.Exchanger, deliveryMethod);
        var localEndpoint = new LocalEndpoint(endpoint, new Fixture().Create<ReceiverDelegate>());
        _endpointsStorage.LocalEndpoints.Add(localEndpoint);
        _endpointsInvoker.Setup(_ => _.InvokeExchanger(It.IsAny<LocalEndpoint>(), It.IsAny<Package>()))
            .Returns(responsePackage);
        _sendingMiddlewaresRunner.Setup(_ => _.Process(responsePackage))
            .Callback<Package>(package => package.Serialized = UTF8.GetBytes("serialized-package"));
        _connectedPeers.Add(_netPeer.Object);

        //Act
        _packageHandler.Handle(requestPackage);

        //Assert
        _endpointsInvoker.Verify(_ => _.InvokeExchanger(localEndpoint, requestPackage));
        responsePackage.Route.Should().Be(new Route("some-route"));
        responsePackage.ExchangeId.Should().Be(responsePackage.ExchangeId);
        responsePackage.IsResponse.Should().Be(true);
        responsePackage.Context.Should().Be(_context.Object);
        responsePackage.ToPeerId.Should().Be(PeerId);
        responsePackage.DeliveryMethod.Should().Be(deliveryMethod);
        _sendingMiddlewaresRunner.Verify(_ => _.Process(responsePackage), Once);
        _netPeer.Verify(_ => _.Send(responsePackage));
    }

    [Test]
    public void Handle_NonExistentEndpoint_Throw()
    {
        // Arrange
        var package = new Package { Route = new Route("some-route") };

        //Act
        Action action = () => _packageHandler.Handle(package);

        //Assert
        action.Should().Throw<FatNetLibException>()
            .WithMessage("Package from  pointed to a non-existent endpoint. Route: some-route");
    }

    [Test, AutoData]
    public void Handle_WrongDeliveryMethod_Throw(string route)
    {
        // Arrange
        var endpoint = new Endpoint(new Route(route), EndpointType.Exchanger, DeliveryMethod.Sequenced);
        var localEndpoint = new LocalEndpoint(endpoint, new Fixture().Create<ReceiverDelegate>());
        _endpointsStorage.LocalEndpoints.Add(localEndpoint);

        var package = new Package
        {
            Route = new Route(route),
            FromPeerId = PeerId,
            DeliveryMethod = DeliveryMethod.Unreliable
        };

        //Act
        Action action = () => _packageHandler.Handle(package);

        //Assert
        action.Should().Throw<FatNetLibException>()
            .WithMessage($"Package from {PeerId} came with the wrong type of delivery");
    }
}