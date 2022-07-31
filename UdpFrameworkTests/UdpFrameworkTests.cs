using System;
using System.Collections.Generic;
using Kolyhalov.UdpFramework;
using Kolyhalov.UdpFramework.Attributes;
using LiteNetLib;
using Moq;
using NUnit.Framework;
using static Kolyhalov.UdpFramework.UdpFramework;

namespace UdpFrameworkTests;

public class UdpFrameworkTests
{
    private UdpFrameworkShell? _udpFrameworkShell;

    [SetUp]
    public void Setup()
    {
        _udpFrameworkShell = new UdpFrameworkShell();
    }

    [Test]
    public void AddController_ControllerWithTwoEndpoints_AddTwoEndpoints()
    {
        // Arrange
        var udpFramework = new UdpFrameworkShell();
        IController controller = new SomeController();

        // Act
        udpFramework.AddController(controller);

        // Assert
        Assert.AreEqual(2, udpFramework.LocalEndpointsCount);
        Assert.AreEqual("Route/correct-route1", udpFramework.LocalEndpointsShell[0].EndpointData.Path);
        Assert.AreEqual(DeliveryMethod.Sequenced, udpFramework.LocalEndpointsShell[1].EndpointData.DeliveryMethod);
        Assert.AreEqual(controller, udpFramework.LocalEndpointsShell[1].Controller);
    }

    [Test]
    public void AddController_NullControllerRoute_Throw()
    {
        // Arrange
        IController controller = new ControllerWithNullRoute();

        // Act
        void Action() => _udpFrameworkShell!.AddController(controller);

        // Assert
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>().With.Message.Contains(
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
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>().With.Message.Contains(
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
            .With.Message.Contains("EndpointWithNullRoute path in ControllerWithNullEndpointRoute is null or empty"));
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
            .With.Message.Contains("EndpointWithEmptyRoute path in ControllerWithEmptyEndpointRoute is null or empty"));
    }


    [Test]
    public void AddController_WrongExchangeReturnType_Throw()
    {
        // Arrange
        IController controller = new ControllerWithWrongExchangerEndpoint();

        // Act
        void Action() => _udpFrameworkShell!.AddController(controller);

        // Assert
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>().With.Message.Contains(
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
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>().With.Message.Contains(
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
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>().With.Message.Contains(
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
            .With.Message.Contains("Endpoint with this path : /correct-route already registered"));
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

        var udpFramework = new UdpFrameworkShell();

        // Act
        udpFramework.AddReceiver(route, deliveryMethod, ReceiverDelegate);

        // Assert
        Assert.AreEqual(1, udpFramework.LocalEndpointsCount);
    }

    [Test]
    public void AddReceiver_NullRoute_Throw()
    {
        // Act
        void Action() => _udpFrameworkShell!
            .AddReceiver(route: null!, It.IsAny<DeliveryMethod>(), receiverDelegate: null!);

        // Assert
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>().With.Message.Contains("Route is null or empty"));
    }

    [Test]
    public void AddReceiver_EmptyRoute_Throw()
    {
        // Act
        void Action() => _udpFrameworkShell!
            .AddReceiver(string.Empty, It.IsAny<DeliveryMethod>(), receiverDelegate: null!);

        // Assert
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>().With.Message.Contains("Route is null or empty"));
    }

    [Test]
    public void AddReceiver_NullReceiverDelegate_Throw()
    {
        // Act
        void Action() => _udpFrameworkShell
            .AddReceiver("correct-route", It.IsAny<DeliveryMethod>(), receiverDelegate: null!);

        // Assert
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>().With.Message.Contains("Method is null is null"));
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

        var udpFramework = new UdpFrameworkShell();
        udpFramework.AddReceiver(route, deliveryMethod, ReceiverDelegate);

        // Act
        void Action() => udpFramework.AddReceiver(route, deliveryMethod, ReceiverDelegate);

        // Assert
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>()
            .With.Message.Contains("Endpoint with the path correct-route was already registered"));
    }

    [Test]
    public void AddExchanger_BuilderStyleExchanger_AddEndpoint()
    {
        // Arrange
        var route = "correct-route";
        var deliveryMethod = DeliveryMethod.Sequenced;
        Package ExchangerDelegate(Package _) => null!;
        var udpFramework = new UdpFrameworkShell();

        // Act
        udpFramework.AddExchanger(route, deliveryMethod, ExchangerDelegate);

        // Assert
        Assert.AreEqual(1, udpFramework.LocalEndpointsCount);
    }

    [Test]
    public void AddExchanger_NullRoute_Throw()
    {
        // Act
        void Action() => _udpFrameworkShell!
            .AddExchanger(route: null!, It.IsAny<DeliveryMethod>(), exchangerDelegate: null!);

        // Assert
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>().With.Message.Contains("Route is null or empty"));
    }

    [Test]
    public void AddExchanger_EmptyRoute_Throw()
    {
        // Act
        void Action() => _udpFrameworkShell!
            .AddExchanger(string.Empty, It.IsAny<DeliveryMethod>(), exchangerDelegate: null!);

        // Assert
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>().With.Message.Contains("Route is null or empty"));
    }

    [Test]
    public void AddExchanger_NullReceiverDelegate_Throw()
    {
        // Act
        void Action() => _udpFrameworkShell!
            .AddExchanger("correct-route", It.IsAny<DeliveryMethod>(), exchangerDelegate: null!);

        // Assert
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>().With.Message.Contains("Method is null is null"));
    }

    [Test]
    public void AddExchanger_ExistEndpoint_Throw()
    {
        // Arrange
        var route = "correct-route";
        var deliveryMethod = DeliveryMethod.Sequenced;

        Package ExchangerDelegate(Package _) => null!;

        var udpFramework = new UdpFrameworkShell();
        udpFramework.AddExchanger(route, deliveryMethod, ExchangerDelegate);

        // Act
        void Action() => udpFramework.AddExchanger(route, deliveryMethod, ExchangerDelegate);

        // Assert
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>()
            .With.Message.Contains("Endpoint with the path correct-route was already registered"));
    }

    [Test]
    public void StartListen_WorkAlreadyFinished_Throw()
    {
        // Arrange
        var udpFramework = new UdpFrameworkShell();
        udpFramework.Stop();

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

        var udpFrameworkShell = new UdpFrameworkShell();
        udpFrameworkShell.ConnectedPeersShell.Add(netPeerMock.Object);

        string route = It.IsAny<string>();
        var endpoint = new Endpoint(route, It.IsAny<EndpointType>(), It.IsAny<DeliveryMethod>());
        udpFrameworkShell.RemoteEndpointsShell[peerId] = new List<Endpoint> {endpoint};

        var package = new Package {Route = route};

        // Act
        Package? result = udpFrameworkShell.SendPackage(package, 0);

        // Assert
        Assert.AreEqual(null, result);
    }

    [Test]
    public void SendPackage_NotFoundPackageReceiver_Throw()
    {
        // Act
        void Action() => _udpFrameworkShell!.SendPackage(package: null!, It.IsAny<int>());

        // Assert 
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>().With.Message.Contains("Receiver not found"));
    }

    [Test]
    public void SendPackage_NotFoundEndpoint_Throw()
    {
        // Arrange
        var udpFrameworkShell = new UdpFrameworkShell();
        int peerId = It.IsAny<int>();
        var netPeerMock = new Mock<INetPeerShell>();
        netPeerMock.Setup(netPeer => netPeer.Id).Returns(peerId);
        udpFrameworkShell.ConnectedPeersShell.Add(netPeerMock.Object);
        udpFrameworkShell.RemoteEndpointsShell[peerId] = new List<Endpoint>();

        // Act
        void Action() => udpFrameworkShell.SendPackage(package: null!, 0);

        // Assert 
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>().With.Message.Contains("Endpoint not found"));
    }

    #region resource classes

    private class UdpFrameworkShell : UdpFramework
    {
        public UdpFrameworkShell() : base(logger: null!, endpointsInvoker: null!, listener:null!)
        {
        }

        public int LocalEndpointsCount => LocalEndpoints.Count;
        public List<LocalEndpoint> LocalEndpointsShell => LocalEndpoints;
        public List<INetPeerShell> ConnectedPeersShell => ConnectedPeers;
        public Dictionary<int, List<Endpoint>> RemoteEndpointsShell => RemoteEndpoints;
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