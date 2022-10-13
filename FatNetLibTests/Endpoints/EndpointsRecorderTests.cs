using System;
using System.Linq;
using FluentAssertions;
using Kolyhalov.FatNetLib.Attributes;
using Kolyhalov.FatNetLib.Microtypes;
using LiteNetLib;
using Moq;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Endpoints;

public class EndpointsRecorderTests
{
    private IEndpointRecorder _endpointRecorder = null!;
    private IEndpointsStorage _endpointsStorage = null!;

    [SetUp]
    public void SetUp()
    {
        _endpointsStorage = new EndpointsStorage();
        _endpointRecorder = new EndpointRecorder(_endpointsStorage);
    }

    [Test]
    public void AddController_ControllerWithTwoEndpoints_AddTwoEndpoints()
    {
        // Arrange
        IController controller = new SomeController();

        // Act
        _endpointRecorder.AddController(controller);

        // Assert
        Endpoint[] result = _endpointsStorage.LocalEndpoints.Select(_ => _.EndpointData).ToArray();
        Assert.AreEqual(2, result.Length);
        Assert.False(result[0].IsInitial);
        Assert.False(result[1].IsInitial);
        Assert.NotNull(_endpointsStorage.LocalEndpoints
            .FirstOrDefault(endpoint => endpoint.EndpointData.Route.Equals(new Route("Route/correct-route1"))));
        Assert.NotNull(_endpointsStorage.LocalEndpoints
            .FirstOrDefault(endpoint => endpoint.EndpointData.Route.Equals(new Route("Route/correct-route2"))));
        Assert.AreEqual(DeliveryMethod.Sequenced, result[0].DeliveryMethod);
        Assert.AreEqual(DeliveryMethod.Sequenced, result[1].DeliveryMethod);
    }

    [Test]
    public void AddController_InitialController_AddTwoInitialEndpoints()
    {
        // Arrange
        IController controller = new InitialController();

        // Act
        _endpointRecorder.AddController(controller);

        // Assert
        Endpoint[] result = _endpointsStorage.LocalEndpoints.Select(_ => _.EndpointData).ToArray();
        Assert.AreEqual(2, result.Length);
        Assert.True(result[0].IsInitial);
        Assert.True(result[1].IsInitial);
        Assert.NotNull(_endpointsStorage.LocalEndpoints
            .FirstOrDefault(endpoint => endpoint.EndpointData.Route.Equals(new Route("correct-route1"))));
        Assert.NotNull(_endpointsStorage.LocalEndpoints
            .FirstOrDefault(endpoint => endpoint.EndpointData.Route.Equals(new Route("correct-route2"))));
        Assert.AreEqual(DeliveryMethod.Sequenced, result[0].DeliveryMethod);
        Assert.AreEqual(DeliveryMethod.Sequenced, result[1].DeliveryMethod);
    }

    [Test]
    public void AddController_InitialControllerWithReceiver_Throw()
    {
        // Arrange
        IController controller = new InitialControllerWithReceiver();

        // Act
        Action action = () => _endpointRecorder.AddController(controller);

        // Assert
        action.Should()
            .Throw<FatNetLibException>()
            .WithMessage("All endpoints of initial controller should be exchanger");
    }

    [Test]
    public void AddController_NullControllerRoute_Throw()
    {
        // Arrange
        IController controller = new ControllerWithNullRoute();

        // Act
        void Action() => _endpointRecorder.AddController(controller);

        // Assert
        Assert.That(Action, Throws.TypeOf<ArgumentException>().With.Message.EqualTo(
            "Route is null or blank"));
    }

    [Test]
    public void AddController_EmptyControllerRoute_Throw()
    {
        // Arrange
        IController controller = new ControllerWithEmptyRoute();

        // Act
        void Action() => _endpointRecorder.AddController(controller);

        // Assert
        Assert.That(Action, Throws.TypeOf<ArgumentException>().With.Message.EqualTo(
            "Route is null or blank"));
    }

    [Test]
    public void AddController_BlancControllerRoute_Throw()
    {
        // Arrange
        IController controller = new ControllerWithBlankRoute();

        // Act
        void Action() => _endpointRecorder.AddController(controller);

        // Assert
        Assert.That(Action, Throws.TypeOf<ArgumentException>().With.Message.EqualTo(
            "Route is null or blank"));
    }

    [Test]
    public void AddController_NullEndpointRoute_Throw()
    {
        // Arrange
        IController controller = new ControllerWithNullEndpointRoute();

        // Act
        void Action() => _endpointRecorder.AddController(controller);

        // Assert
        Assert.That(Action, Throws.TypeOf<ArgumentException>()
            .With.Message.EqualTo("Route is null or blank"));
    }

    [Test]
    public void AddController_EmptyEndpointRoute_Throw()
    {
        // Arrange
        IController controller = new ControllerWithEmptyEndpointRoute();

        // Act
        void Action() => _endpointRecorder.AddController(controller);

        // Assert
        Assert.That(Action, Throws.TypeOf<ArgumentException>()
            .With.Message.EqualTo("Route is null or blank"));
    }

    [Test]
    public void AddController_BlankEndpointRoute_Throw()
    {
        // Arrange
        IController controller = new ControllerWithBlankEndpointRoute();

        // Act
        void Action() => _endpointRecorder.AddController(controller);

        // Assert
        Assert.That(Action, Throws.TypeOf<ArgumentException>()
            .With.Message.EqualTo("Route is null or blank"));
    }


    [Test]
    public void AddController_WrongExchangeReturnType_Throw()
    {
        // Arrange
        IController controller = new ControllerWithWrongExchangerEndpoint();

        // Act
        void Action() => _endpointRecorder.AddController(controller);

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
        void Action() => _endpointRecorder.AddController(controller);

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
        void Action() => _endpointRecorder.AddController(controller);

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
        void Action() => _endpointRecorder.AddController(controller);

        // Assert
        Assert.That(Action, Throws.TypeOf<FatNetLibException>()
            .With.Message.EqualTo("Endpoint with the route correct-route was already registered"));
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
        _endpointRecorder.AddReceiver(route, deliveryMethod, ReceiverDelegate);

        // Assert
        Endpoint[] result = _endpointsStorage.LocalEndpoints.Select(_ => _.EndpointData).ToArray();
        Assert.NotNull(
            _endpointsStorage.LocalEndpoints.FirstOrDefault(_ => _.EndpointData.Route.Equals(new Route(route))));
        Assert.False(result[0].IsInitial);
        Assert.AreEqual(1, result.Length);
        Assert.AreEqual(deliveryMethod, result[0].DeliveryMethod);
    }

    [Test]
    public void AddReceiver_NullRoute_Throw()
    {
        // Act
        void Action() => _endpointRecorder
            .AddReceiver(route: (Route) null!, It.IsAny<DeliveryMethod>(), receiverDelegate: null!);

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
        void Action() => _endpointRecorder
            .AddReceiver(route: string.Empty, It.IsAny<DeliveryMethod>(), ReceiverDelegate);

        // Assert
        Assert.That(Action, Throws.TypeOf<ArgumentException>().With.Message
            .Contains("Route is null or blank"));
    }

    [Test]
    public void AddReceiver_BlankRoute_Throw()
    {
        // Arrange
        void ReceiverDelegate(Package _)
        {
        }

        // Act
        void Action() => _endpointRecorder
            .AddReceiver(route: "  ", It.IsAny<DeliveryMethod>(), ReceiverDelegate);

        // Assert
        Assert.That(Action, Throws.TypeOf<ArgumentException>()
            .With.Message.Contains("Route is null or blank"));
    }

    [Test]
    public void AddReceiver_NullReceiverDelegate_Throw()
    {
        // Act
        void Action() => _endpointRecorder
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

        _endpointRecorder.AddReceiver(route, deliveryMethod, ReceiverDelegate);

        // Act
        void Action() => _endpointRecorder.AddReceiver(route, deliveryMethod, ReceiverDelegate);

        // Assert
        Assert.That(Action, Throws.TypeOf<FatNetLibException>()
            .With.Message.Contains("Endpoint with the route : correct-route was already registered"));
    }

    [Test]
    public void AddExchanger_BuilderStylerExchanger_AddEndpoint()
    {
        // Arrange
        var route = "correct-route";
        var deliveryMethod = DeliveryMethod.Sequenced;

        Package ExchangerDelegate(Package _) => null!;

        // Act
        _endpointRecorder.AddExchanger(route, deliveryMethod, ExchangerDelegate);

        // Assert
        Endpoint[] result = _endpointsStorage.LocalEndpoints.Select(_ => _.EndpointData).ToArray();
        Assert.False(result[0].IsInitial);
        Assert.NotNull(
            _endpointsStorage.LocalEndpoints.FirstOrDefault(_ => _.EndpointData.Route.Equals(new Route(route))));
        Assert.AreEqual(1, result.Length);
        Assert.AreEqual(deliveryMethod, result[0].DeliveryMethod);
    }

    [Test]
    public void AddExchanger_ExchangerAsInitial_AddInitialEndpoint()
    {
        // Arrange
        var route = "correct-route";
        var deliveryMethod = DeliveryMethod.Sequenced;

        Package ExchangerDelegate(Package _) => null!;

        // Act
        _endpointRecorder.AddExchanger(route, deliveryMethod, ExchangerDelegate, isInitial: true);

        // Assert
        Endpoint[] result = _endpointsStorage.LocalEndpoints.Select(_ => _.EndpointData).ToArray();
        Assert.True(result[0].IsInitial);
        Assert.NotNull(
            _endpointsStorage.LocalEndpoints.FirstOrDefault(_ => _.EndpointData.Route.Equals(new Route(route))));
        Assert.AreEqual(1, result.Length);
        Assert.AreEqual(deliveryMethod, result[0].DeliveryMethod);
    }

    [Test]
    public void AddExchanger_NullRoute_Throw()
    {
        // Act
        void Action() => _endpointRecorder
            .AddExchanger(route: (Route) null!, It.IsAny<DeliveryMethod>(), exchangerDelegate: null!);

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
        void Action() => _endpointRecorder
            .AddExchanger(route: string.Empty, It.IsAny<DeliveryMethod>(), ExchangerDelegate);

        // Assert
        Assert.That(Action, Throws.TypeOf<ArgumentException>().With.Message
            .Contains("Route is null or blank"));
    }

    [Test]
    public void AddExchanger_BlankRoute_Throw()
    {
        // Arrange
        Package ExchangerDelegate(Package _) => null!;

        // Act
        void Action() => _endpointRecorder
            .AddExchanger(route: "  ", It.IsAny<DeliveryMethod>(), ExchangerDelegate);

        // Assert
        Assert.That(Action, Throws.TypeOf<ArgumentException>().With.Message
            .Contains("Route is null or blank"));
    }

    [Test]
    public void AddExchanger_NullExchangerDelegate_Throw()
    {
        // Act
        void Action() => _endpointRecorder
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

        _endpointRecorder.AddExchanger(route, deliveryMethod, ExchangerDelegate);

        // Act
        void Action() => _endpointRecorder.AddExchanger(route, deliveryMethod, ExchangerDelegate);

        // Assert
        Assert.That(Action, Throws.TypeOf<FatNetLibException>()
            .With.Message.Contains("Endpoint with the route : correct-route was already registered"));
    }


    #region test classes

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

    [Initial]
    private class InitialController : IController
    {
        [Route("correct-route1")]
        [Exchanger(DeliveryMethod.Sequenced)]
        public Package SomeEndpoint1() => null!;

        [Route("correct-route2")]
        [Exchanger(DeliveryMethod.Sequenced)]
        public Package SomeEndpoint2() => null!;
    }

    [Initial]
    private class InitialControllerWithReceiver : IController
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

    [Route(route: null!)]
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
        [Route(route: null!)]
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
}