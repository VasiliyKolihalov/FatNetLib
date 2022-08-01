using System;
using System.Collections.Generic;
using System.Linq;
using Kolyhalov.UdpFramework;
using Kolyhalov.UdpFramework.Attributes;
using LiteNetLib;
using Moq;
using NUnit.Framework;

namespace UdpFrameworkTests;

public class UdpFrameworkTests
{
    private UdpFrameworkShell? _udpFrameworkShell;

    [SetUp]
    public void Setup()
    {
        _udpFrameworkShell = new UdpFrameworkShell(new EndpointsStorage());
    }

    [Test]
    public void AddController_ControllerWithTwoEndpoints_AddTwoEndpoints()
    {
        // Arrange
        var udpFramework = _udpFrameworkShell;
        IController controller = new SomeController();

        // Act
        udpFramework!.AddController(controller);

        // Assert
        Assert.AreEqual(2, udpFramework.EndpointsStorageShell.GetLocalEndpointsData().Count());
        Assert.NotNull(udpFramework.EndpointsStorageShell.GetLocalEndpointFromPath("Route/correct-route1"));
        Assert.AreEqual(DeliveryMethod.Sequenced,
            udpFramework.EndpointsStorageShell.GetLocalEndpointsData().ToArray()[0].DeliveryMethod);
    }

    [Test]
    public void AddController_NullControllerRoute_Throw()
    {
        // Arrange
        IController controller = new ControllerWithNullRoute();

        // Act
        void Action() => _udpFrameworkShell!.AddController(controller);

        // Assert
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>().With.Message.EqualTo(
            "UdpFrameworkTests.UdpFrameworkTests+ControllerWithNullRoute path is null or empty"));
    }

    [Test]
    public void AddController_EmptyControllerRoute_Throw()
    {
        // Arrange
        IController controller = new ControllerWithEmptyRoute();

        // Act
        void Action() => _udpFrameworkShell!.AddController(controller);

        // Assert
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>().With.Message.EqualTo(
            "UdpFrameworkTests.UdpFrameworkTests+ControllerWithEmptyRoute path is null or empty"));
    }

    [Test]
    public void AddController_EndpointRouteNull_Throw()
    {
        // Arrange
        IController controller = new ControllerWithNullEndpointRoute();

        // Act
        void Action() => _udpFrameworkShell!.AddController(controller);

        // Assert
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>()
            .With.Message.EqualTo("EndpointWithNullRoute path in ControllerWithNullEndpointRoute is null or empty"));
    }

    [Test]
    public void AddController_EmptyEndpointRoute_Throw()
    {
        // Arrange
        IController controller = new ControllerWithEmptyEndpointRoute();

        // Act
        void Action() => _udpFrameworkShell!.AddController(controller);

        // Assert
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>()
            .With.Message.EqualTo("EndpointWithEmptyRoute path in ControllerWithEmptyEndpointRoute is null or empty"));
    }


    [Test]
    public void AddController_WrongExchangeReturnType_Throw()
    {
        // Arrange
        IController controller = new ControllerWithWrongExchangerEndpoint();

        // Act
        void Action() => _udpFrameworkShell!.AddController(controller);

        // Assert
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>().With.Message.EqualTo(
            "Return type of a WrongExchangerEndpoint in a ControllerWithWrongExchangerEndpoint must be package"));
    }

    [Test]
    public void AddController_EndpointWithoutRoute_Throw()
    {
        // Arrange
        IController controller = new ControllerWithEndpointWithoutRoute();

        // Act
        void Action() => _udpFrameworkShell!.AddController(controller);

        // Assert
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>().With.Message.EqualTo(
            "EndpointWithoutRoute in ControllerWithEndpointWithoutRoute does not have route attribute"));
    }

    [Test]
    public void AddController_EndpointWithoutType_Throw()
    {
        // Arrange
        IController controller = new ControllerWithEndpointWithoutType();

        // Act
        void Action() => _udpFrameworkShell!.AddController(controller);

        // Assert
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>().With.Message.EqualTo(
            "EndpointWithoutType in ControllerWithEndpointWithoutType does not have endpoint type attribute"));
    }

    [Test]
    public void AddController_TwoEndpointsWithSameRoute_Throw()
    {
        // Arrange
        IController controller = new ControllerWithEndpointsWithSameRoute();

        // Act
        void Action() => _udpFrameworkShell!.AddController(controller);

        // Assert
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>()
            .With.Message.EqualTo("Endpoint with the path : /correct-route was already registered"));
    }

    [Test]
    public void AddReceiver_BuilderStyleReceiver_AddEndpoint()
    {
        // Arrange
        var route = "correct-route";
        var deliveryMethod = DeliveryMethod.Sequenced;

        void ReceiverDelegate(Package _)
        {
        }

        var endpointsStorage = new EndpointsStorage();
        var udpFramework = new UdpFrameworkShell(endpointsStorage);

        // Act
        udpFramework!.AddReceiver(route, deliveryMethod, ReceiverDelegate);

        // Assert
        Assert.NotNull(udpFramework.EndpointsStorageShell.GetLocalEndpointFromPath("correct-route"));
        Assert.NotNull(endpointsStorage.GetLocalEndpointFromPath(route));
        Assert.AreEqual(1, endpointsStorage.GetLocalEndpointsData().Count());
        Assert.AreEqual(deliveryMethod, endpointsStorage.GetLocalEndpointsData().ToArray()[0].DeliveryMethod);
    }

    [Test]
    public void AddReceiver_NullRoute_Throw()
    {
        // Act
        void Action() => _udpFrameworkShell!
            .AddReceiver(route: null!, It.IsAny<DeliveryMethod>(), receiverDelegate: null!);

        // Assert
        Assert.That(Action, Throws.TypeOf<ArgumentNullException>()
            .With.Message.Contains("Value cannot be null. (Parameter 'route')"));
    }

    [Test]
    public void AddReceiver_EmptyRoute_Throw()
    {
        // Arrange
        void ReceiverDelegate(Package _)
        {
        }

        // Act
        void Action() => _udpFrameworkShell!
            .AddReceiver(string.Empty, It.IsAny<DeliveryMethod>(), ReceiverDelegate);

        // Assert
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>().With.Message
            .Contains("Path is null or white space"));
    }

    [Test]
    public void AddReceiver_SpaceRoute_Throw()
    {
        // Arrange
        void ReceiverDelegate(Package package)
        {
        }

        // Act
        void Action() => _udpFrameworkShell!
            .AddReceiver("  ", It.IsAny<DeliveryMethod>(), ReceiverDelegate);

        // Assert
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>()
            .With.Message.Contains("Path is null or white space"));
    }

    [Test]
    public void AddReceiver_NullReceiverDelegate_Throw()
    {
        // Act
        void Action() => _udpFrameworkShell!
            .AddReceiver("correct-route", It.IsAny<DeliveryMethod>(), receiverDelegate: null!);

        // Assert
        Assert.That(Action, Throws.TypeOf<ArgumentNullException>().With.Message
            .Contains("Value cannot be null. (Parameter 'receiverDelegate')"));
    }

    [Test]
    public void AddReceiver_ExistEndpoint_Throw()
    {
        // Arrange
        var route = "correct-route";
        var deliveryMethod = DeliveryMethod.Sequenced;

        void ReceiverDelegate(Package _)
        {
        }

        var udpFramework = _udpFrameworkShell;
        udpFramework!.AddReceiver(route, deliveryMethod, ReceiverDelegate);

        // Act
        void Action() => udpFramework.AddReceiver(route, deliveryMethod, ReceiverDelegate);

        // Assert
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>()
            .With.Message.Contains("Endpoint with the path : correct-route was already registered"));
    }

    [Test]
    public void AddExchanger_BuilderStylerExchanger_AddEndpoint()
    {
        // Arrange
        var route = "correct-route";
        var deliveryMethod = DeliveryMethod.Sequenced;

        Package ExchangerDelegate(Package _) => null!;

        var endpointsStorage = new EndpointsStorage();
        var udpFramework = new UdpFrameworkShell(endpointsStorage);

        // Act
        udpFramework!.AddExchanger(route, deliveryMethod, ExchangerDelegate);

        // Assert
        Assert.NotNull(udpFramework.EndpointsStorageShell.GetLocalEndpointFromPath("correct-route"));
        Assert.NotNull(endpointsStorage.GetLocalEndpointFromPath(route));
        Assert.AreEqual(1, endpointsStorage.GetLocalEndpointsData().Count());
        Assert.AreEqual(deliveryMethod, endpointsStorage.GetLocalEndpointsData().ToArray()[0].DeliveryMethod);
    }

    [Test]
    public void AddExchanger_NullRoute_Throw()
    {
        // Act
        void Action() => _udpFrameworkShell!
            .AddExchanger(route: null!, It.IsAny<DeliveryMethod>(), exchangerDelegate: null!);

        // Assert
        Assert.That(Action, Throws.TypeOf<ArgumentNullException>()
            .With.Message.Contains("Value cannot be null. (Parameter 'route')"));
    }

    [Test]
    public void AddExchanger_EmptyRoute_Throw()
    {
        // Arrange
        Package ExchangerDelegate(Package _) => null!;

        // Act
        void Action() => _udpFrameworkShell!
            .AddExchanger(string.Empty, It.IsAny<DeliveryMethod>(), ExchangerDelegate);

        // Assert
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>().With.Message
            .Contains("Path is null or white space"));
    }

    [Test]
    public void AddExchanger_SpaceRoute_Throw()
    {
        // Arrange
        Package ExchangerDelegate(Package _) => null!;

        // Act
        void Action() => _udpFrameworkShell!
            .AddExchanger(route: "  ", It.IsAny<DeliveryMethod>(), ExchangerDelegate);

        // Assert
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>().With.Message
            .Contains("Path is null or white space"));
    }

    [Test]
    public void AddExchanger_NullReceiverDelegate_Throw()
    {
        // Act
        void Action() => _udpFrameworkShell!
            .AddExchanger("correct-route", It.IsAny<DeliveryMethod>(), exchangerDelegate: null!);

        // Assert
        Assert.That(Action, Throws.TypeOf<ArgumentNullException>().With.Message
            .Contains("Value cannot be null. (Parameter 'exchangerDelegate')"));
    }

    [Test]
    public void AddExchanger_ExistEndpoint_Throw()
    {
        // Arrange
        var route = "correct-route";
        var deliveryMethod = DeliveryMethod.Sequenced;

        Package ExchangerDelegate(Package _) => null!;

        var udpFramework = _udpFrameworkShell;
        udpFramework!.AddExchanger(route, deliveryMethod, ExchangerDelegate);

        // Act
        void Action() => udpFramework.AddExchanger(route, deliveryMethod, ExchangerDelegate);

        // Assert
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>()
            .With.Message.Contains("Endpoint with the path : correct-route was already registered"));
    }

    [Test]
    public void StartListen_WorkAlreadyFinished_Throw()
    {
        // Arrange
        var udpFramework = _udpFrameworkShell;
        udpFramework!.Stop();

        // Act
        void Action() => udpFramework.StartListenShell();

        // Assert
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>().With.Message.Contains("UdpFramework finished work"));
    }

    [Test]
    public void SendPackage_ToReceiver_ReturnNull()
    {
        // Arrange
        var netPeerMock = new Mock<INetPeerShell>();
        int peerId = It.IsAny<int>();
        netPeerMock.Setup(netPeer => netPeer.Id).Returns(peerId);

        var route = "correct-route";
        var endpoint = new Endpoint(route, It.IsAny<EndpointType>(), It.IsAny<DeliveryMethod>());
        var endpointsStorage = new EndpointsStorage();
        endpointsStorage.AddRemoteEndpoints(peerId, new List<Endpoint> {endpoint});
        var udpFrameworkShell = new UdpFrameworkShell(endpointsStorage);
        udpFrameworkShell.ConnectedPeersShell.Add(netPeerMock.Object);

        var package = new Package {Route = route};

        // Act
        Package? result = udpFrameworkShell.SendPackage(package, 0);

        // Assert
        Assert.AreEqual(null, result);
    }

    [Test]
    public void SendPackage_NullRouteInPackage_Throw()
    {
        // Act
        void Action() => _udpFrameworkShell!.SendPackage(new Package(), It.IsAny<int>());

        // Assert 
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>().With.Message.Contains("Route is null"));
    }

    [Test]
    public void SendPackage_NotFoundPackageReceiver_Throw()
    {
        // Act
        void Action() => _udpFrameworkShell!.SendPackage(new Package() {Route = "correct-route"}, It.IsAny<int>());

        // Assert 
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>().With.Message.Contains("Receiver not found"));
    }

    [Test]
    public void SendPackage_NotFoundEndpoint_Throw()
    {
        // Arrange
        var endpointsStorage = new EndpointsStorage();
        int peerId = It.IsAny<int>();
        endpointsStorage.AddRemoteEndpoints(peerId, new List<Endpoint>());
        var netPeerMock = new Mock<INetPeerShell>();
        netPeerMock.Setup(netPeer => netPeer.Id).Returns(peerId);
        var udpFrameworkShell = new UdpFrameworkShell(endpointsStorage);
        udpFrameworkShell.ConnectedPeersShell.Add(netPeerMock.Object);

        // Act
        void Action() => udpFrameworkShell.SendPackage(new Package() {Route = "correct-route"}, peerId);

        // Assert 
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>().With.Message.Contains("Endpoint not found"));
    }

    #region resource classes

    private class UdpFrameworkShell : UdpFramework
    {
        public IEndpointsStorage EndpointsStorageShell { get; }
        public List<INetPeerShell> ConnectedPeersShell => ConnectedPeers;

        public UdpFrameworkShell(IEndpointsStorage endpointsStorage)
            : base(logger: null!, endpointsInvoker: null!, listener: null!, endpointsStorage: endpointsStorage)
        {
            EndpointsStorageShell = endpointsStorage;
        }

        public void StartListenShell() => StartListen(It.IsAny<int>());

        public override void Run() => throw new NotImplementedException();
    }

    [Route("Route")]
    private class SomeController : IController
    {
        [Route("correct-route1")]
        [Receiver(DeliveryMethod.Sequenced)]
        public void SomeEndpoint1()
        {
        }

        [Route("correct-route2")]
        [Exchanger(DeliveryMethod.Sequenced)]
        public Package SomeEndpoint2() => null!;
    }

    [Route(path: null!)]
    private class ControllerWithNullRoute : IController
    {
    }

    [Route("")]
    private class ControllerWithEmptyRoute : IController
    {
    }

    private class ControllerWithNullEndpointRoute : IController
    {
        [Route(path: null!)]
        public void EndpointWithNullRoute()
        {
        }
    }

    private class ControllerWithEmptyEndpointRoute : IController
    {
        [Route("")]
        public void EndpointWithEmptyRoute()
        {
        }
    }

    private class ControllerWithWrongExchangerEndpoint : IController
    {
        [Route("correct-route")]
        [Exchanger(DeliveryMethod.Sequenced)]
        public void WrongExchangerEndpoint()
        {
        }
    }

    private class ControllerWithEndpointWithoutRoute : IController
    {
        public void EndpointWithoutRoute()
        {
        }
    }

    private class ControllerWithEndpointWithoutType : IController
    {
        [Route("correct-route")]
        public void EndpointWithoutType()
        {
        }
    }

    private class ControllerWithEndpointsWithSameRoute : IController
    {
        [Route("correct-route")]
        [Receiver(DeliveryMethod.Sequenced)]
        public void SomeEndpoint1()
        {
        }

        [Route("correct-route")]
        [Exchanger(DeliveryMethod.Sequenced)]
        public Package SomeEndpoint2() => null!;
    }

    #endregion
}