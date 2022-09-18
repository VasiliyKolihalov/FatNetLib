using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using Kolyhalov.FatNetLib;
using Kolyhalov.FatNetLib.Attributes;
using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Middlewares;
using Kolyhalov.FatNetLib.NetPeers;
using Kolyhalov.FatNetLib.ResponsePackageMonitors;
using LiteNetLib;
using LiteNetLib.Utils;
using Moq;
using NUnit.Framework;
using static Moq.Times;

namespace FatNetLibTests;

public class FatNetLibTests
{
    //TODO: rewrite tests for past refactoring
    
    /*
    private EndpointsStorage _endpointsStorage = null!;
    private FatNetLibShell _fatNetLib = null!;
    private Mock<INetPeer> _netPeer = null!;
    private Mock<IResponsePackageMonitor> _responsePackageMonitor = null!;
    private Mock<IMiddlewaresRunner> _sendingMiddlewaresRunner = null!;
    private Mock<IMiddlewaresRunner> _receivingMiddlewaresRunner = null!;

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
        _fatNetLib = new FatNetLibShell(_endpointsStorage,
            _responsePackageMonitor.Object,
            _sendingMiddlewaresRunner.Object,
            _receivingMiddlewaresRunner.Object);
    }

    [Test]
    public void AddController_ControllerWithTwoEndpoints_AddTwoEndpoints()
    {
        // Arrange
        IController controller = new SomeController();

        // Act
        _fatNetLib.AddController(controller);

        // Assert
        Endpoint[] result = _fatNetLib.EndpointsStorage.LocalEndpoints.Select(_ => _.EndpointData).ToArray();
        Assert.AreEqual(2, result.Length);
        Assert.NotNull(_endpointsStorage.LocalEndpoints
            .FirstOrDefault(endpoint => endpoint.EndpointData.Path == "Route/correct-route1"));
        Assert.NotNull(_endpointsStorage.LocalEndpoints
            .FirstOrDefault(endpoint => endpoint.EndpointData.Path == "Route/correct-route2"));
        Assert.AreEqual(DeliveryMethod.Sequenced, result[0].DeliveryMethod);
        Assert.AreEqual(DeliveryMethod.Sequenced, result[1].DeliveryMethod);
    }

    [Test]
    public void AddController_NullControllerRoute_Throw()
    {
        // Arrange
        IController controller = new ControllerWithNullRoute();

        // Act
        void Action() => _fatNetLib.AddController(controller);

        // Assert
        Assert.That(Action, Throws.TypeOf<FatNetLibException>().With.Message.EqualTo(
            "FatNetLibTests.FatNetLibTests+ControllerWithNullRoute path is null or blank"));
    }

    [Test]
    public void AddController_EmptyControllerRoute_Throw()
    {
        // Arrange
        IController controller = new ControllerWithEmptyRoute();

        // Act
        void Action() => _fatNetLib.AddController(controller);

        // Assert
        Assert.That(Action, Throws.TypeOf<FatNetLibException>().With.Message.EqualTo(
            "FatNetLibTests.FatNetLibTests+ControllerWithEmptyRoute path is null or blank"));
    }

    [Test]
    public void AddController_BlancControllerRoute_Throw()
    {
        // Arrange
        IController controller = new ControllerWithBlankRoute();

        // Act
        void Action() => _fatNetLib.AddController(controller);

        // Assert
        Assert.That(Action, Throws.TypeOf<FatNetLibException>().With.Message.EqualTo(
            "FatNetLibTests.FatNetLibTests+ControllerWithBlankRoute path is null or blank"));
    }

    [Test]
    public void AddController_NullEndpointRoute_Throw()
    {
        // Arrange
        IController controller = new ControllerWithNullEndpointRoute();

        // Act
        void Action() => _fatNetLib.AddController(controller);

        // Assert
        Assert.That(Action, Throws.TypeOf<FatNetLibException>()
            .With.Message.EqualTo("EndpointWithNullRoute path in ControllerWithNullEndpointRoute is null or blank"));
    }

    [Test]
    public void AddController_EmptyEndpointRoute_Throw()
    {
        // Arrange
        IController controller = new ControllerWithEmptyEndpointRoute();

        // Act
        void Action() => _fatNetLib.AddController(controller);

        // Assert
        Assert.That(Action, Throws.TypeOf<FatNetLibException>()
            .With.Message.EqualTo("EndpointWithEmptyRoute path in ControllerWithEmptyEndpointRoute is null or blank"));
    }

    [Test]
    public void AddController_BlankEndpointRoute_Throw()
    {
        // Arrange
        IController controller = new ControllerWithBlankEndpointRoute();

        // Act
        void Action() => _fatNetLib.AddController(controller);

        // Assert
        Assert.That(Action, Throws.TypeOf<FatNetLibException>()
            .With.Message.EqualTo("EndpointWithEmptyRoute path in ControllerWithBlankEndpointRoute is null or blank"));
    }


    [Test]
    public void AddController_WrongExchangeReturnType_Throw()
    {
        // Arrange
        IController controller = new ControllerWithWrongExchangerEndpoint();

        // Act
        void Action() => _fatNetLib.AddController(controller);

        // Assert
        Assert.That(Action, Throws.TypeOf<FatNetLibException>().With.Message.EqualTo(
            "Return type of a WrongExchangerEndpoint in a ControllerWithWrongExchangerEndpoint must be Package"));
    }

    [Test]
    public void AddController_EndpointWithoutRoute_Throw()
    {
        // Arrange
        IController controller = new ControllerWithEndpointWithoutRoute();

        // Act
        void Action() => _fatNetLib.AddController(controller);

        // Assert
        Assert.That(Action, Throws.TypeOf<FatNetLibException>().With.Message.EqualTo(
            "EndpointWithoutRoute in ControllerWithEndpointWithoutRoute does not have route attribute"));
    }

    [Test]
    public void AddController_EndpointWithoutType_Throw()
    {
        // Arrange
        IController controller = new ControllerWithEndpointWithoutType();

        // Act
        void Action() => _fatNetLib.AddController(controller);

        // Assert
        Assert.That(Action, Throws.TypeOf<FatNetLibException>().With.Message.EqualTo(
            "EndpointWithoutType in ControllerWithEndpointWithoutType does not have endpoint type attribute"));
    }

    [Test]
    public void AddController_TwoEndpointsWithSameRoute_Throw()
    {
        // Arrange
        IController controller = new ControllerWithEndpointsWithSameRoute();

        // Act
        void Action() => _fatNetLib.AddController(controller);

        // Assert
        Assert.That(Action, Throws.TypeOf<FatNetLibException>()
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

        // Act
        _fatNetLib.AddReceiver(route, deliveryMethod, ReceiverDelegate);

        // Assert
        Endpoint[] result = _endpointsStorage.LocalEndpoints.Select(_ => _.EndpointData).ToArray();
        Assert.NotNull(_endpointsStorage.LocalEndpoints.FirstOrDefault(_ => _.EndpointData.Path == route));
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual(deliveryMethod, result[0].DeliveryMethod);
    }

    [Test]
    public void AddReceiver_NullRoute_Throw()
    {
        // Act
        void Action() => _fatNetLib
            .AddReceiver(route: null!, It.IsAny<DeliveryMethod>(), receiverDelegate: null!);

        // Assert
        Assert.That(Action, Throws.TypeOf<ArgumentException>()
            .With.Message.Contains("Path is null or blank"));
    }

    [Test]
    public void AddReceiver_EmptyRoute_Throw()
    {
        // Arrange
        void ReceiverDelegate(Package _)
        {
        }

        // Act
        void Action() => _fatNetLib
            .AddReceiver(route: string.Empty, It.IsAny<DeliveryMethod>(), ReceiverDelegate);

        // Assert
        Assert.That(Action, Throws.TypeOf<ArgumentException>().With.Message
            .Contains("Path is null or blank"));
    }

    [Test]
    public void AddReceiver_BlankRoute_Throw()
    {
        // Arrange
        void ReceiverDelegate(Package _)
        {
        }

        // Act
        void Action() => _fatNetLib
            .AddReceiver(route: "  ", It.IsAny<DeliveryMethod>(), ReceiverDelegate);

        // Assert
        Assert.That(Action, Throws.TypeOf<ArgumentException>()
            .With.Message.Contains("Path is null or blank"));
    }

    [Test]
    public void AddReceiver_NullReceiverDelegate_Throw()
    {
        // Act
        void Action() => _fatNetLib
            .AddReceiver("correct-route", It.IsAny<DeliveryMethod>(), receiverDelegate: null!);

        // Assert
        Assert.That(Action, Throws.TypeOf<ArgumentNullException>().With.Message
            .Contains("Value cannot be null. (Parameter 'methodDelegate')"));
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

        _fatNetLib.AddReceiver(route, deliveryMethod, ReceiverDelegate);

        // Act
        void Action() => _fatNetLib.AddReceiver(route, deliveryMethod, ReceiverDelegate);

        // Assert
        Assert.That(Action, Throws.TypeOf<FatNetLibException>()
            .With.Message.Contains("Endpoint with the path : correct-route was already registered"));
    }

    [Test]
    public void AddExchanger_BuilderStylerExchanger_AddEndpoint()
    {
        // Arrange
        var route = "correct-route";
        var deliveryMethod = DeliveryMethod.Sequenced;

        Package ExchangerDelegate(Package _) => null!;

        // Act
        _fatNetLib.AddExchanger(route, deliveryMethod, ExchangerDelegate);

        // Assert
        Endpoint[] result = _endpointsStorage.LocalEndpoints.Select(_ => _.EndpointData).ToArray();
        Assert.NotNull(_endpointsStorage.LocalEndpoints.FirstOrDefault(_ => _.EndpointData.Path == route));
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual(deliveryMethod, result[0].DeliveryMethod);
    }

    [Test]
    public void AddExchanger_NullRoute_Throw()
    {
        // Act
        void Action() => _fatNetLib
            .AddExchanger(route: null!, It.IsAny<DeliveryMethod>(), exchangerDelegate: null!);

        // Assert
        Assert.That(Action, Throws.TypeOf<ArgumentException>()
            .With.Message.Contains("Path is null or blank"));
    }

    [Test]
    public void AddExchanger_EmptyRoute_Throw()
    {
        // Arrange
        Package ExchangerDelegate(Package _) => null!;

        // Act
        void Action() => _fatNetLib
            .AddExchanger(route: string.Empty, It.IsAny<DeliveryMethod>(), ExchangerDelegate);

        // Assert
        Assert.That(Action, Throws.TypeOf<ArgumentException>().With.Message
            .Contains("Path is null or blank"));
    }

    [Test]
    public void AddExchanger_BlankRoute_Throw()
    {
        // Arrange
        Package ExchangerDelegate(Package _) => null!;

        // Act
        void Action() => _fatNetLib
            .AddExchanger(route: "  ", It.IsAny<DeliveryMethod>(), ExchangerDelegate);

        // Assert
        Assert.That(Action, Throws.TypeOf<ArgumentException>().With.Message
            .Contains("Path is null or blank"));
    }

    [Test]
    public void AddExchanger_NullExchangerDelegate_Throw()
    {
        // Act
        void Action() => _fatNetLib
            .AddExchanger("correct-route", It.IsAny<DeliveryMethod>(), exchangerDelegate: null!);

        // Assert
        Assert.That(Action, Throws.TypeOf<ArgumentNullException>().With.Message
            .Contains("Value cannot be null. (Parameter 'methodDelegate')"));
    }

    [Test]
    public void AddExchanger_ExistEndpoint_Throw()
    {
        // Arrange
        var route = "correct-route";
        var deliveryMethod = DeliveryMethod.Sequenced;

        Package ExchangerDelegate(Package _) => null!;

        _fatNetLib.AddExchanger(route, deliveryMethod, ExchangerDelegate);

        // Act
        void Action() => _fatNetLib.AddExchanger(route, deliveryMethod, ExchangerDelegate);

        // Assert
        Assert.That(Action, Throws.TypeOf<FatNetLibException>()
            .With.Message.Contains("Endpoint with the path : correct-route was already registered"));
    }

    [Test]
    public void StartListen_WorkAlreadyFinished_Throw()
    {
        // Arrange
        _fatNetLib.Stop();

        // Act
        void Action() => _fatNetLib.StartListen();

        // Assert
        Assert.That(Action, Throws.TypeOf<FatNetLibException>().With.Message.Contains("FatNetLib finished work"));
    }

    [Test]
    public void SendPackage_ToReceivingPeer_SendAndReturnNull()
    {
        // Arrange
        var route = "correct-route";
        var deliveryMethod = DeliveryMethod.Sequenced;
        var endpoint = new Endpoint(route, EndpointType.Receiver, deliveryMethod);
        _endpointsStorage.RemoteEndpoints[NetPeerId] = new List<Endpoint> {endpoint};
        _fatNetLib.ConnectedPeers.Add(_netPeer.Object);

        var package = new Package {Route = route};

        // Act
        Package? result = _fatNetLib.SendPackage(package, NetPeerId);

        // Assert
        Assert.AreEqual(null, result);
        _netPeer.Verify(netPeer => netPeer.Send(It.IsAny<NetDataWriter>(), deliveryMethod));
    }

    [Test]
    public void SendPackage_NullPackage_Throw()
    {
        // Act
        void Action() => _fatNetLib.SendPackage(package: null!, It.IsAny<int>());

        // Assert 
        Assert.That(Action,
            Throws.TypeOf<ArgumentNullException>().With.Message
                .Contains("Value cannot be null. (Parameter 'package')"));
    }

    [Test]
    public void SendPackage_NotFoundReceivingPeer_Throw()
    {
        // Act
        void Action() => _fatNetLib.SendPackage(new Package() {Route = "correct-route"}, It.IsAny<int>());

        // Assert 
        Assert.That(Action, Throws.TypeOf<FatNetLibException>().With.Message.Contains("Receiving peer not found"));
    }

    [Test]
    public void SendPackage_NotFoundEndpoint_Throw()
    {
        // Arrange
        _endpointsStorage.RemoteEndpoints[NetPeerId] = new List<Endpoint>();
        _fatNetLib.ConnectedPeers.Add(_netPeer.Object);

        // Act
        void Action() => _fatNetLib.SendPackage(new Package() {Route = "correct-route"}, NetPeerId);

        // Assert 
        Assert.That(Action, Throws.TypeOf<FatNetLibException>().With.Message.Contains("Endpoint not found"));
    }

    [Test]
    public void SendPackage_ToReceivingPeer_SendingMiddlewareRunnerCalled()
    {
        // Arrange
        var endpoint = new Endpoint("correct-route", EndpointType.Receiver, DeliveryMethod.Sequenced);
        _endpointsStorage.RemoteEndpoints[NetPeerId] = new List<Endpoint> { endpoint };
        _fatNetLib.ConnectedPeers.Add(_netPeer.Object);
        var package = new Package { Route = "correct-route" };

        // Act
        _fatNetLib.SendPackage(package, NetPeerId);

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
        _endpointsStorage.RemoteEndpoints[NetPeerId] = new List<Endpoint> {endpoint};
        _fatNetLib.ConnectedPeers.Add(_netPeer.Object);
        var requestPackage = new Package {Route = "correct-route", ExchangeId = Guid.NewGuid()};
        var expectedResponsePackage = new Package();
        _responsePackageMonitor.Setup(m => m.Wait(It.IsAny<Guid>()))
            .Returns(expectedResponsePackage);

        // Act
        Package? actualResponsePackage = _fatNetLib.SendPackage(requestPackage, NetPeerId);

        // Assert
        actualResponsePackage.Should().Be(expectedResponsePackage);
        _netPeer.Verify(netPeer => netPeer.Send(It.IsAny<NetDataWriter>(), DeliveryMethod.Sequenced));
        _responsePackageMonitor.Verify(m => m.Wait(It.IsAny<Guid>()), Once);
        _responsePackageMonitor.Verify(m => m.Wait(
            It.Is<Guid>(exchangeId => exchangeId == requestPackage.ExchangeId)));
    }

    [Test]
    public void SendPackage_ToExchangerWithoutExchangeId_GenerateExchangeId()
    {
        // Arrange
        var endpoint = new Endpoint("correct-route", EndpointType.Exchanger, DeliveryMethod.Sequenced);
        _endpointsStorage.RemoteEndpoints.Add(NetPeerId, new List<Endpoint> {endpoint});
        _fatNetLib.ConnectedPeers.Add(_netPeer.Object);
        var requestPackage = new Package {Route = "correct-route", ExchangeId = null};
        _responsePackageMonitor.Setup(m => m.Wait(It.IsAny<Guid>()))
            .Returns(new Func<Guid, Package>(exchangeId => new Package() {ExchangeId = exchangeId}));

        // Act
        Package? actualResponsePackage = _fatNetLib.SendPackage(requestPackage, NetPeerId);

        // Assert
        actualResponsePackage!.ExchangeId.Should().NotBeNull();
    }

    [Test]
    public void SendPackage_ToExchangingPeer_SendingAndReceivingMiddlewareRunnersCalled()
    {
        // Arrange
        var endpoint = new Endpoint("correct-route", EndpointType.Exchanger, DeliveryMethod.Sequenced);
        _endpointsStorage.RemoteEndpoints[NetPeerId] = new List<Endpoint> { endpoint };
        _fatNetLib.ConnectedPeers.Add(_netPeer.Object);
        var requestPackage = new Package { Route = "correct-route", ExchangeId = Guid.NewGuid() };

        // Act
        Package? responsePackage = _fatNetLib.SendPackage(requestPackage, NetPeerId);

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
        _fatNetLib.ConnectedPeers.Add(_netPeer.Object);
        var requestPackage = new Package { Route = "correct-route", ExchangeId = Guid.NewGuid() };

        var initialResponsePackage = new Package() { ExchangeId = Guid.NewGuid() };
        _responsePackageMonitor.Setup(_ => _.Wait(It.IsAny<Guid>()))
            .Returns(initialResponsePackage);

        var replacedResponsePackage = new Package();
        _receivingMiddlewaresRunner.Setup(_ => _.Process(initialResponsePackage))
            .Returns(replacedResponsePackage);

        // Act
        Package? actualResponsePackage = _fatNetLib.SendPackage(requestPackage, NetPeerId);

        // Assert
        actualResponsePackage.Should().Be(replacedResponsePackage);
    }

    private Mock<IMiddlewaresRunner> APassingMiddlewareRunner()
    {
        var middlewareRunner = new Mock<IMiddlewaresRunner>();
        middlewareRunner.Setup(_ => _.Process(It.IsAny<Package>()))
            .Returns<Package>((package) => package);
        return middlewareRunner;
    }
    */

    /*#region resource classes

    private class FatNetLibShell : FatNetLib
    {
        public new IEndpointsStorage EndpointsStorage => base.EndpointsStorage;
        public new IList<INetPeer> ConnectedPeers => base.ConnectedPeers;
        protected override Configuration Configuration => throw new InvalidOperationException();

        public FatNetLibShell(IEndpointsStorage endpointsStorage,
            IResponsePackageMonitor responsePackageMonitor,
            IMiddlewaresRunner sendingMiddlewaresRunner,
            IMiddlewaresRunner receivingMiddlewaresRunner)
            : base(logger: null!,
                endpointsInvoker: null!,
                listener: null!,
                endpointsStorage: endpointsStorage,
                responsePackageMonitor: responsePackageMonitor,
                sendingMiddlewaresRunner: sendingMiddlewaresRunner,
                receivingMiddlewaresRunner: receivingMiddlewaresRunner)
        {
        }

        public new void StartListen() => base.StartListen();

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

    [Route("  ")]
    private class ControllerWithBlankRoute : IController
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

    private class ControllerWithBlankEndpointRoute : IController
    {
        [Route("  ")]
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
*/
}