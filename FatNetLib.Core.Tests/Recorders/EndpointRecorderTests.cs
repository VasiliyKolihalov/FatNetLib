using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Attributes;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Controllers;
using Kolyhalov.FatNetLib.Core.Delegates;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Recorders;
using Kolyhalov.FatNetLib.Core.Storages;
using NUnit.Framework;
using static Kolyhalov.FatNetLib.Core.Tests.Recorders.EndpointRecorderTests.TestControllers;

namespace Kolyhalov.FatNetLib.Core.Tests.Recorders
{
    public class EndpointRecorderTests
    {
        private readonly ConsumerAction _consumerAction = _ => { };
        private readonly ExchangerAction _exchangerAction = _ => null!;
        private readonly EventAction _eventAction = _ => { };
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
            Endpoint[] result = _endpointsStorage.LocalEndpoints.Select(_ => _.Details).ToArray();
            Assert.AreEqual(2, result.Length);
            Assert.NotNull(_endpointsStorage.LocalEndpoints
                .FirstOrDefault(endpoint => endpoint.Details.Route.Equals(new Route("Route/correct-route1"))));
            Assert.NotNull(_endpointsStorage.LocalEndpoints
                .FirstOrDefault(endpoint => endpoint.Details.Route.Equals(new Route("Route/correct-route2"))));
            Assert.AreEqual(EndpointType.Consumer, result[0].Type);
            Assert.AreEqual(EndpointType.Exchanger, result[1].Type);
            Assert.AreEqual(Reliability.Sequenced, result[0].Reliability);
            Assert.AreEqual(Reliability.Sequenced, result[1].Reliability);
        }

        [Test]
        public void AddController_InitialController_AddTwoExchangers()
        {
            // Arrange
            IController controller = new InitialController();

            // Act
            _endpointRecorder.AddController(controller);

            // Assert
            Endpoint[] result = _endpointsStorage.LocalEndpoints.Select(_ => _.Details).ToArray();
            Assert.AreEqual(2, result.Length);
            Assert.NotNull(_endpointsStorage.LocalEndpoints
                .FirstOrDefault(endpoint => endpoint.Details.Route.Equals(new Route("correct-route1"))));
            Assert.NotNull(_endpointsStorage.LocalEndpoints
                .FirstOrDefault(endpoint => endpoint.Details.Route.Equals(new Route("correct-route2"))));
            Assert.AreEqual(EndpointType.Initializer, result[0].Type);
            Assert.AreEqual(EndpointType.Initializer, result[1].Type);
            Assert.AreEqual(Reliability.ReliableOrdered, result[0].Reliability);
            Assert.AreEqual(Reliability.ReliableOrdered, result[1].Reliability);
        }

        [Test]
        public void AddController_EventController_AddTwoEventEndpoints()
        {
            // Arrange
            IController controller = new EventController();

            // Act
            _endpointRecorder.AddController(controller);

            // Assert
            Endpoint[] result = _endpointsStorage.LocalEndpoints.Select(_ => _.Details).ToArray();
            Assert.AreEqual(2, result.Length);
            Assert.NotNull(_endpointsStorage.LocalEndpoints
                .FirstOrDefault(endpoint => endpoint.Details.Route.Equals(new Route("correct-route1"))));
            Assert.NotNull(_endpointsStorage.LocalEndpoints
                .FirstOrDefault(endpoint => endpoint.Details.Route.Equals(new Route("correct-route2"))));
            Assert.AreEqual(EndpointType.Event, result[0].Type);
            Assert.AreEqual(EndpointType.Event, result[1].Type);
            Assert.AreEqual(Reliability.ReliableOrdered, result[0].Reliability);
            Assert.AreEqual(Reliability.ReliableOrdered, result[1].Reliability);
        }

        [Test]
        public void AddController_Null_Throw()
        {
            // Act
            void Action() => _endpointRecorder.AddController(null!);

            // Assert
            Assert.That(Action, Throws.TypeOf<ArgumentNullException>().With.Message.EqualTo(
                "Value cannot be null. (Parameter 'controller')"));
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
            IController controller = new ControllerWithWrongExchangers();

            // Act
            void Action() => _endpointRecorder.AddController(controller);

            // Assert
            Assert.That(Action, Throws.TypeOf<FatNetLibException>().With.Message.EqualTo(
                "Return type of exchanger or initial endpoint cannot be void. Endpoint route: correct-route"));
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
        public void AddController_WithSchemaPatch_Add()
        {
            // Act
            _endpointRecorder.AddController(new ControllerWithSchemaPatch());

            // Assert
            _endpointsStorage.LocalEndpoints[0].Details
                .RequestSchemaPatch
                .Should().BeEquivalentTo(new PackageSchema { { "AuthToken", typeof(Guid) } });
            _endpointsStorage.LocalEndpoints[0].Details
                .ResponseSchemaPatch
                .Should().BeEquivalentTo(new PackageSchema { { "Body", typeof(EndpointsBody) } });
        }

        [Test]
        public void AddController_WithParameterAttributesAndSchemaPatch_AddWithSchema()
        {
            // Act
            _endpointRecorder.AddController(new ControllerWithParameterAttributesAndSchemaPatch());

            // Assert
            _endpointsStorage.LocalEndpoints[0].Details
                .RequestSchemaPatch
                .Should().BeEquivalentTo(new PackageSchema { { "Error", typeof(string) }, { "Body", typeof(int) } });
        }

        [Test]
        public void AddController_WithNonPackageReturnType_AddWithSchema()
        {
            // Act
            _endpointRecorder.AddController(new ControllerWithNonPackageReturnType());

            // Assert
            _endpointsStorage.LocalEndpoints[0].Details
                .ResponseSchemaPatch
                .Should().BeEquivalentTo(new PackageSchema { { "Body", typeof(Guid?) } });
        }

        [Test]
        public void AddEndpoint_BuilderStyleConsumer_Add()
        {
            // Act
            _endpointRecorder.AddConsumer(new Route("correct-route"), _consumerAction);

            // Assert
            Endpoint[] result = _endpointsStorage.LocalEndpoints.Select(_ => _.Details).ToArray();
            Assert.NotNull(
                _endpointsStorage.LocalEndpoints.FirstOrDefault(
                    _ => _.Details.Route.Equals(new Route("correct-route"))));
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(Reliability.ReliableOrdered, result[0].Reliability);
            Assert.AreEqual(EndpointType.Consumer, result[0].Type);
        }

        [Test]
        public void AddEndpoint_BuilderStylerExchanger_Add()
        {
            // Act
            _endpointRecorder.AddExchanger(new Route("correct-route"), _exchangerAction);

            // Assert
            Endpoint[] result = _endpointsStorage.LocalEndpoints.Select(_ => _.Details).ToArray();
            Assert.NotNull(
                _endpointsStorage.LocalEndpoints.FirstOrDefault(
                    _ => _.Details.Route.Equals(new Route("correct-route"))));
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(EndpointType.Exchanger, result[0].Type);
        }

        [Test]
        public void AddEndpoint_BuilderStyleInitializer_Add()
        {
            // Act
            _endpointRecorder.AddInitial(new Route("correct-route"), _exchangerAction);

            // Assert
            Endpoint[] result = _endpointsStorage.LocalEndpoints.Select(_ => _.Details).ToArray();
            Assert.NotNull(
                _endpointsStorage.LocalEndpoints.FirstOrDefault(
                    _ => _.Details.Route.Equals(new Route("correct-route"))));
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(Reliability.ReliableOrdered, result[0].Reliability);
            Assert.AreEqual(EndpointType.Initializer, result[0].Type);
        }

        [Test]
        public void AddEndpoint_NullRoute_Throw()
        {
            // Act
            void Action() => _endpointRecorder
                .AddConsumer(route: null!, _consumerAction);

            // Assert
            Assert.That(Action, Throws.TypeOf<ArgumentNullException>()
                .With.Message.Contains("Value cannot be null. (Parameter 'route')"));
        }

        [Test]
        public void AddEndpoint_NullDelegate_Throw()
        {
            // Act
            void Action() => _endpointRecorder
                .AddConsumer(new Route("correct-route"), action: null!);

            // Assert
            Assert.That(Action, Throws.TypeOf<ArgumentNullException>().With.Message
                .Contains("Value cannot be null. (Parameter 'endpointDelegate')"));
        }

        [Test]
        public void AddEndpoint_ExistingEndpoint_Throw()
        {
            _endpointRecorder.AddConsumer(new Route("correct-route"), _consumerAction);

            // Act
            void Action() => _endpointRecorder.AddConsumer(new Route("correct-route"), _consumerAction);

            // Assert
            Assert.That(Action, Throws.TypeOf<FatNetLibException>()
                .With.Message.Contains("Endpoint with the route correct-route was already registered"));
        }

        [Test]
        public void AddEndpoint_WithSchemaPatch_Add()
        {
            // Act
            _endpointRecorder.AddExchanger(
                new Route("correct-route"),
                _exchangerAction,
                requestSchemaPatch: new PackageSchema { { "AuthToken", typeof(Guid) } },
                responseSchemaPatch: new PackageSchema { { "Body", typeof(EndpointsBody) } });

            // Assert
            _endpointsStorage.LocalEndpoints[0].Details
                .RequestSchemaPatch
                .Should().BeEquivalentTo(new PackageSchema { { "AuthToken", typeof(Guid) } });
            _endpointsStorage.LocalEndpoints[0].Details
                .ResponseSchemaPatch
                .Should().BeEquivalentTo(new PackageSchema { { "Body", typeof(EndpointsBody) } });
        }

        [Test]
        public void AddEvent_CorrectCase_Pass()
        {
            // Arrange
            var route1 = new Route("correct-route");
            var route2 = new Route("correct-route");

            // Act
            _endpointRecorder
                .AddEvent(route1, _eventAction)
                .AddEvent(route2, _eventAction);

            // Assert
            _endpointsStorage.LocalEndpoints[0].Details.Route.Should().BeEquivalentTo(route1);
            _endpointsStorage.LocalEndpoints[0].Details.Type.Should().Be(EndpointType.Event);
            _endpointsStorage.LocalEndpoints[0].Action.Should().BeEquivalentTo(_eventAction);
            _endpointsStorage.LocalEndpoints[1].Details.Route.Should().BeEquivalentTo(route2);
            _endpointsStorage.LocalEndpoints[1].Details.Type.Should().Be(EndpointType.Event);
            _endpointsStorage.LocalEndpoints[1].Action.Should().BeEquivalentTo(_eventAction);
        }

        [Test]
        public void AddEvent_NullRoute_Throw()
        {
            // Act
            Action act = () => _endpointRecorder.AddEvent(route: null!, _eventAction);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage("Value cannot be null. (Parameter 'route')");
        }

        [Test]
        public void AddEvent_NullAction_Throw()
        {
            // Act
            Action act = () => _endpointRecorder.AddEvent(new Route("correct-route"), null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithMessage("Value cannot be null. (Parameter 'action')");
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        [SuppressMessage("Performance", "CA1822:Mark members as static")]
        internal static class TestControllers
        {
            [Route("Route")]
            public class SomeController : IController
            {
                [Route("correct-route1")]
                [Consumer(Reliability.Sequenced)]
                public void SomeEndpoint1()
                {
                }

                [Route("correct-route2")]
                [Exchanger(Reliability.Sequenced)]
                public Package SomeEndpoint2() => null!;
            }

            public class InitialController : IController
            {
                [Initializer]
                [Route("correct-route1")]
                public Package SomeEndpoint1() => null!;

                [Initializer]
                [Route("correct-route2")]
                public Package SomeEndpoint2() => null!;
            }

            public class EventController : IController
            {
                [Event]
                [Route("correct-route1")]
                public void SomeEndpoint1()
                {
                }

                [Event]
                [Route("correct-route2")]
                public void SomeEndpoint2()
                {
                }
            }

            [Route(route: null!)]
            public class ControllerWithNullRoute : IController
            {
            }

            [Route("")]
            public class ControllerWithEmptyRoute : IController
            {
            }

            [Route("  ")]
            public class ControllerWithBlankRoute : IController
            {
            }

            public class ControllerWithNullEndpointRoute : IController
            {
                [Route(route: null!)]
                public void EndpointWithNullRoute()
                {
                }
            }

            public class ControllerWithEmptyEndpointRoute : IController
            {
                [Route("")]
                public void EndpointWithEmptyRoute()
                {
                }
            }

            public class ControllerWithBlankEndpointRoute : IController
            {
                [Route("  ")]
                public void EndpointWithEmptyRoute()
                {
                }
            }

            public class ControllerWithWrongExchangers : IController
            {
                [Route("correct-route")]
                [Exchanger(Reliability.Sequenced)]
                public void WrongExchanger()
                {
                }
            }

            public class ControllerWithEndpointWithoutRoute : IController
            {
                public void EndpointWithoutRoute()
                {
                }
            }

            public class ControllerWithEndpointWithoutType : IController
            {
                [Route("correct-route")]
                public void EndpointWithoutType()
                {
                }
            }

            public class ControllerWithEndpointsWithSameRoute : IController
            {
                [Route("correct-route")]
                [Consumer(Reliability.Sequenced)]
                public void SomeEndpoint1()
                {
                }

                [Route("correct-route")]
                [Exchanger(Reliability.Sequenced)]
                public Package SomeEndpoint2() => null!;
            }

            public class ControllerWithSchemaPatch : IController
            {
                [Route("correct-route")]
                [Consumer]
                [Schema("AuthToken", typeof(Guid))]
                [return: Schema("Body", typeof(EndpointsBody))]
                public void SomeEndpoint()
                {
                }
            }

            public class ControllerWithParameterAttributesAndSchemaPatch : IController
            {
                [Route("correct-route")]
                [Consumer]
                [Schema(nameof(Package.Body), typeof(int))]
                public void SomeEndpoint([Body] Guid id, [Error] string errorMessage)
                {
                }
            }

            public class ControllerWithNonPackageReturnType : IController
            {
                [Route("correct-route")]
                [Exchanger]
                public Guid? SomeEndpoint() => null;
            }
        }
    }
}
