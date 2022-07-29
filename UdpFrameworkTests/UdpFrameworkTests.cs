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
    [Test]
    public void AddControllerShouldAddAllEndpoints()
    {
        UdpFrameworkShell udpFramework = new UdpFrameworkShell();
        IController controller = new SomeController();

        udpFramework.AddController(controller);

        int endpointsCount = typeof(SomeController).GetMethods(EndpointSearch).Length;
        Assert.AreEqual(endpointsCount, udpFramework.LocalEndpointsCount);
    }

    [Test]
    public void AddControllerShouldThrowBecauseRouteNull()
    {
        IController controller = new ControllerWithNullRoute();

        TestDelegate action = () => new UdpFrameworkShell().AddController(controller);

        Assert.Throws<UdpFrameworkException>(action);
    }

    [Test]
    public void AddControllerShouldThrowBecauseRouteEmpty()
    {
        IController controller = new ControllerWithEmptyRoute();

        TestDelegate action = () => new UdpFrameworkShell().AddController(controller);

        Assert.Throws<UdpFrameworkException>(action);
    }

    [Test]
    public void AddControllerShouldThrowBecauseEndpointRouteNull()
    {
        IController controller = new ControllerWithNullEndpointRoute();

        TestDelegate action = () => new UdpFrameworkShell().AddController(controller);

        Assert.Throws<UdpFrameworkException>(action);
    }

    [Test]
    public void AddControllerShouldThrowBecauseEndpointRouteEmpty()
    {
        IController controller = new ControllerWithEmptyEndpointRoute();

        TestDelegate action = () => new UdpFrameworkShell().AddController(controller);

        Assert.Throws<UdpFrameworkException>(action);
    }


    [Test]
    public void AddControllerShouldThrowBecauseExchangeReturnTypeWrong()
    {
        IController controller = new ControllerWithWrongExchangerEndpoint();

        TestDelegate action = () => new UdpFrameworkShell().AddController(controller);

        Assert.Throws<UdpFrameworkException>(action);
    }

    [Test]
    public void AddControllerShouldThrowBecauseEndpointWithoutRoute()
    {
        IController controller = new ControllerWithEndpointWithoutRoute();

        TestDelegate action = () => new UdpFrameworkShell().AddController(controller);

        Assert.Throws<UdpFrameworkException>(action);
    }

    [Test]
    public void AddControllerShouldThrowBecauseEndpointWithoutType()
    {
        IController controller = new ControllerWithEndpointWithoutType();

        TestDelegate action = () => new UdpFrameworkShell().AddController(controller);

        Assert.Throws<UdpFrameworkException>(action);
    }

    [Test]
    public void AddControllerShouldThrowBecauseEndpointsHaveSameRoute()
    {
        IController controller = new ControllerWithEndpointsWithSameRoute();

        TestDelegate action = () => new UdpFrameworkShell().AddController(controller);

        Assert.Throws<UdpFrameworkException>(action);
    }

    [Test]
    public void AddReceiverShouldAddEndpoint()
    {
        string route = "correct-route";
        DeliveryMethod deliveryMethod = DeliveryMethod.Sequenced;
        ReceiverDelegate receiverDelegate = _ => { };
        UdpFrameworkShell udpFramework = new UdpFrameworkShell();

        udpFramework.AddReceiver(route, deliveryMethod, receiverDelegate);

        Assert.AreEqual(1, udpFramework.LocalEndpointsCount);
    }

    [Test]
    public void AddReceiverShouldThrowBecauseRouteNull()
    {
        TestDelegate action = () => new UdpFrameworkShell().AddReceiver(null!, It.IsAny<DeliveryMethod>(), null!);

        Assert.Throws<UdpFrameworkException>(action);
    }

    [Test]
    public void AddReceiverShouldThrowBecauseRouteEmpty()
    {
        TestDelegate action = () =>
            new UdpFrameworkShell().AddReceiver(string.Empty, It.IsAny<DeliveryMethod>(), null!);

        Assert.Throws<UdpFrameworkException>(action);
    }

    [Test]
    public void AddReceiverShouldThrowBecauseReceiverDelegateNull()
    {
        TestDelegate action = () =>
            new UdpFrameworkShell().AddReceiver("correct-route", It.IsAny<DeliveryMethod>(), null!);

        Assert.Throws<UdpFrameworkException>(action);
    }

    [Test]
    public void AddReceiverShouldThrowBecauseEndpointAlreadyExist()
    {
        string route = "correct-route";
        DeliveryMethod deliveryMethod = DeliveryMethod.Sequenced;
        ReceiverDelegate receiverDelegate = _ => { };
        UdpFrameworkShell udpFramework = new UdpFrameworkShell();
        udpFramework.AddReceiver(route, deliveryMethod, receiverDelegate);

        TestDelegate action = () => udpFramework.AddReceiver(route, deliveryMethod, receiverDelegate);

        Assert.Throws<UdpFrameworkException>(action);
    }

    [Test]
    public void AddExchangerShouldAddEndpoint()
    {
        string route = "correct-route";
        DeliveryMethod deliveryMethod = DeliveryMethod.Sequenced;
        ExchangerDelegate exchangerDelegate = _ => null!;
        UdpFrameworkShell udpFramework = new UdpFrameworkShell();

        udpFramework.AddExchanger(route, deliveryMethod, exchangerDelegate);

        Assert.AreEqual(1, udpFramework.LocalEndpointsCount);
    }

    [Test]
    public void AddExchangerShouldThrowBecauseRouteNull()
    {
        TestDelegate action = () => new UdpFrameworkShell().AddExchanger(null!, It.IsAny<DeliveryMethod>(), null!);

        Assert.Throws<UdpFrameworkException>(action);
    }

    [Test]
    public void AddExchangerShouldThrowBecauseRouteEmpty()
    {
        TestDelegate action = () =>
            new UdpFrameworkShell().AddExchanger(string.Empty, It.IsAny<DeliveryMethod>(), null!);

        Assert.Throws<UdpFrameworkException>(action);
    }

    [Test]
    public void AddExchangerShouldThrowBecauseExchangerDelegateNull()
    {
        TestDelegate action = () =>
            new UdpFrameworkShell().AddExchanger("correct-route", It.IsAny<DeliveryMethod>(), null!);

        Assert.Throws<UdpFrameworkException>(action);
    }

    [Test]
    public void AddExchangerShouldThrowBecauseEndpointAlreadyExist()
    {
        string route = "correct-route";
        DeliveryMethod deliveryMethod = DeliveryMethod.Sequenced;
        ExchangerDelegate exchangerDelegate = _ => null!;
        UdpFrameworkShell udpFramework = new UdpFrameworkShell();
        udpFramework.AddExchanger(route, deliveryMethod, exchangerDelegate);

        TestDelegate action = () => udpFramework.AddExchanger(route, deliveryMethod, exchangerDelegate);

        Assert.Throws<UdpFrameworkException>(action);
    }

    [Test]
    public void StartListenShouldThrowBecauseWorkDone()
    {
        UdpFrameworkShell udpFramework = new UdpFrameworkShell();
        udpFramework.Stop();

        TestDelegate action = () => udpFramework.StartListenShell();

        Assert.Throws<UdpFrameworkException>(action);
    }

    [Test]
    public void SendPackageShouldSend()
    {
        Mock<INetPeerShell> netPeerMock = new Mock<INetPeerShell>();
        int peerId = It.IsAny<int>();
        netPeerMock.Setup(x => x.Id).Returns(peerId);
        
        UdpFrameworkShell udpFrameworkShell = new UdpFrameworkShell();
        udpFrameworkShell.ConnectedPeersShell.Add(netPeerMock.Object);
        
        string route = It.IsAny<string>();
        Endpoint endpoint = new Endpoint(route, It.IsAny<EndpointType>(), It.IsAny<DeliveryMethod>());
        udpFrameworkShell.RemoteEndpointsShell[peerId] = new List<Endpoint> {endpoint};
        
        Package package = new Package {Route = route};

        TestDelegate action = () => udpFrameworkShell.SendPackage(package, 0);
        
        Assert.DoesNotThrow(action);
    }

    [Test]
    public void SendPackageShouldThrowBecauseReceiverNotFound()
    {
        TestDelegate action = () => new UdpFrameworkShell().SendPackage(null!, It.IsAny<int>());

        Assert.Throws<UdpFrameworkException>(action);
    }

    [Test]
    public void SendPackageShouldThrowBecauseEndpointNotFound()
    {
        UdpFrameworkShell udpFrameworkShell = new UdpFrameworkShell();
        int peerId = It.IsAny<int>();
        Mock<INetPeerShell> netPeerMock = new Mock<INetPeerShell>();
        netPeerMock.Setup(x => x.Id).Returns(peerId);
        udpFrameworkShell.ConnectedPeersShell.Add(netPeerMock.Object);
        udpFrameworkShell.RemoteEndpointsShell[peerId] = new List<Endpoint>();

        TestDelegate action = () => udpFrameworkShell.SendPackage(null!, 0);

        Assert.Throws<UdpFrameworkException>(action);
    }

    #region resource classes

    private class UdpFrameworkShell : UdpFramework
    {
        public UdpFrameworkShell() : base(null!, null!) { }

        public int LocalEndpointsCount => LocalEndpoints.Count;
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
        public void SomeEndpoint1() { }

        [Route("correct-route2")]
        [Exchanger(DeliveryMethod.Sequenced)]
        public Package SomeEndpoint2() => null!;
    }

    [Route(null!)]
    private class ControllerWithNullRoute : IController { }

    [Route("")]
    private class ControllerWithEmptyRoute : IController { }

    private class ControllerWithNullEndpointRoute : IController
    {
        [Route(null!)]
        public void EndpointWithNullRoute() { }
    }

    private class ControllerWithEmptyEndpointRoute : IController
    {
        [Route("")]
        public void EndpointWithEmptyRoute() { }
    }

    private class ControllerWithWrongExchangerEndpoint : IController
    {
        [Route("correct-route")]
        [Exchanger(DeliveryMethod.Sequenced)]
        public void WrongExchangerEndpoint() { }
    }

    private class ControllerWithEndpointWithoutRoute : IController
    {
        public void EndpointWithoutRoute() { }
    }

    private class ControllerWithEndpointWithoutType : IController
    {
        [Route("correct-route")]
        public void EndpointWithoutType() { }
    }

    private class ControllerWithEndpointsWithSameRoute : IController
    {
        [Route("correct-route")]
        [Receiver(DeliveryMethod.Sequenced)]
        public void SomeEndpoint1() { }

        [Route("correct-route")]
        [Exchanger(DeliveryMethod.Sequenced)]
        public Package SomeEndpoint2() => null!;
    }

    #endregion
}