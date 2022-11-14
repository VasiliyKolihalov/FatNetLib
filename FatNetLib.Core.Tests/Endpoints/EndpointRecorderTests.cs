﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Attributes;
using Kolyhalov.FatNetLib.Core.Endpoints;
using Kolyhalov.FatNetLib.Core.Initializers;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Moq;
using NUnit.Framework;
using static Kolyhalov.FatNetLib.Core.Tests.Endpoints.EndpointRecorderTests.TestControllers;

namespace Kolyhalov.FatNetLib.Core.Tests.Endpoints
{
    public class EndpointRecorderTests
    {
        private readonly ReceiverDelegate _receiverDelegate = _ => { };
        private readonly ExchangerDelegate _exchangerDelegate = _ => null!;
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
            Assert.AreEqual(EndpointType.Receiver, result[0].EndpointType);
            Assert.AreEqual(EndpointType.Exchanger, result[1].EndpointType);
            Assert.AreEqual(Reliability.Sequenced, result[0].Reliability);
            Assert.AreEqual(Reliability.Sequenced, result[1].Reliability);
        }

        [Test]
        public void AddController_InitialController_AddTwoExchangerEndpoints()
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
            Assert.AreEqual(EndpointType.Exchanger, result[0].EndpointType);
            Assert.AreEqual(EndpointType.Exchanger, result[1].EndpointType);
            Assert.AreEqual(Reliability.ReliableOrdered, result[0].Reliability);
            Assert.AreEqual(Reliability.ReliableOrdered, result[1].Reliability);
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
                "Return type of exchanger should be Package"));
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
        public void AddEndpoint_BuilderStyleReceiver_Add()
        {
            // Act
            _endpointRecorder.AddReceiver("correct-route", Reliability.Sequenced, _receiverDelegate);

            // Assert
            Endpoint[] result = _endpointsStorage.LocalEndpoints.Select(_ => _.EndpointData).ToArray();
            Assert.NotNull(
                _endpointsStorage.LocalEndpoints.FirstOrDefault(
                    _ => _.EndpointData.Route.Equals(new Route("correct-route"))));
            Assert.False(result[0].IsInitial);
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(Reliability.Sequenced, result[0].Reliability);
        }

        [Test]
        public void AddEndpoint_BuilderStylerExchanger_Add()
        {
            // Act
            _endpointRecorder.AddExchanger("correct-route", Reliability.Sequenced, _exchangerDelegate);

            // Assert
            Endpoint[] result = _endpointsStorage.LocalEndpoints.Select(_ => _.EndpointData).ToArray();
            Assert.False(result[0].IsInitial);
            Assert.NotNull(
                _endpointsStorage.LocalEndpoints.FirstOrDefault(
                    _ => _.EndpointData.Route.Equals(new Route("correct-route"))));
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(Reliability.Sequenced, result[0].Reliability);
        }

        [Test]
        public void AddEndpoint_InitialEndpoint_Add()
        {
            // Act
            _endpointRecorder.AddInitial("correct-route", _exchangerDelegate);

            // Assert
            Endpoint[] result = _endpointsStorage.LocalEndpoints.Select(_ => _.EndpointData).ToArray();
            Assert.True(result[0].IsInitial);
            Assert.NotNull(
                _endpointsStorage.LocalEndpoints.FirstOrDefault(
                    _ => _.EndpointData.Route.Equals(new Route("correct-route"))));
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(Reliability.ReliableOrdered, result[0].Reliability);
        }

        [Test]
        public void AddEndpoint_NullRoute_Throw()
        {
            // Act
            void Action() => _endpointRecorder
                .AddReceiver(route: (Route)null!, It.IsAny<Reliability>(), _receiverDelegate);

            // Assert
            Assert.That(Action, Throws.TypeOf<ArgumentNullException>()
                .With.Message.Contains("Value cannot be null. (Parameter 'route')"));
        }

        [Test]
        public void AddEndpoint_EmptyRoute_Throw()
        {
            // Act
            void Action() => _endpointRecorder
                .AddReceiver(route: string.Empty, It.IsAny<Reliability>(), _receiverDelegate);

            // Assert
            Assert.That(Action, Throws.TypeOf<ArgumentException>().With.Message
                .Contains("Route is null or blank"));
        }

        [Test]
        public void AddEndpoint_BlankRoute_Throw()
        {
            // Act
            void Action() => _endpointRecorder
                .AddReceiver(route: "  ", It.IsAny<Reliability>(), _receiverDelegate);

            // Assert
            Assert.That(Action, Throws.TypeOf<ArgumentException>()
                .With.Message.Contains("Route is null or blank"));
        }

        [Test]
        public void AddEndpoint_NullDelegate_Throw()
        {
            // Act
            void Action() => _endpointRecorder
                .AddReceiver("correct-route", It.IsAny<Reliability>(), receiverDelegate: null!);

            // Assert
            Assert.That(Action, Throws.TypeOf<ArgumentNullException>().With.Message
                .Contains("Value cannot be null. (Parameter 'endpointDelegate')"));
        }

        [Test]
        public void AddEndpoint_ExistingEndpoint_Throw()
        {
            _endpointRecorder.AddReceiver("correct-route", Reliability.Sequenced, _receiverDelegate);

            // Act
            void Action() => _endpointRecorder.AddReceiver("correct-route", Reliability.Sequenced, _receiverDelegate);

            // Assert
            Assert.That(Action, Throws.TypeOf<FatNetLibException>()
                .With.Message.Contains("Endpoint with the route correct-route was already registered"));
        }

        [Test]
        public void AddController_WithSchemaPatch_Add()
        {
            // Act
            _endpointRecorder.AddController(new ControllerWithSchemaPatch());

            // Assert
            _endpointsStorage.LocalEndpoints[0].EndpointData
                .RequestSchemaPatch
                .Should().BeEquivalentTo(new PackageSchema { { "AuthToken", typeof(Guid) } });
            _endpointsStorage.LocalEndpoints[0].EndpointData
                .ResponseSchemaPatch
                .Should().BeEquivalentTo(new PackageSchema { { "Body", typeof(EndpointsBody) } });
        }

        [Test]
        public void AddEndpoint_WithSchemaPatch_Add()
        {
            // Act
            _endpointRecorder.AddExchanger(
                "correct-route",
                Reliability.Sequenced,
                _exchangerDelegate,
                requestSchemaPatch: new PackageSchema { { "AuthToken", typeof(Guid) } },
                responseSchemaPatch: new PackageSchema { { "Body", typeof(EndpointsBody) } });

            // Assert
            _endpointsStorage.LocalEndpoints[0].EndpointData
                .RequestSchemaPatch
                .Should().BeEquivalentTo(new PackageSchema { { "AuthToken", typeof(Guid) } });
            _endpointsStorage.LocalEndpoints[0].EndpointData
                .ResponseSchemaPatch
                .Should().BeEquivalentTo(new PackageSchema { { "Body", typeof(EndpointsBody) } });
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        [SuppressMessage("Performance", "CA1822:Mark members as static")]
        internal static class TestControllers
        {
            [Route("Route")]
            public class SomeController : IController
            {
                [Route("correct-route1")]
                [Receiver(Reliability.Sequenced)]
                public void SomeEndpoint1()
                {
                }

                [Route("correct-route2")]
                [Exchanger(Reliability.Sequenced)]
                public Package SomeEndpoint2() => null!;
            }

            [Initial]
            public class InitialController : IController
            {
                [Route("correct-route1")]
                public Package SomeEndpoint1() => null!;

                [Route("correct-route2")]
                public Package SomeEndpoint2() => null!;
            }

            [Initial]
            public class InitialControllerWithReceiver : IController
            {
                [Route("correct-route1")]
                [Receiver]
                public void SomeEndpoint1()
                {
                }

                [Route("correct-route2")]
                [Exchanger]
                public Package SomeEndpoint2() => null!;
            }

            [Initial]
            public class InitialControllerWithWrongEndpointReliability : IController
            {
                [Route("correct-route1")]
                [Exchanger(Reliability.Unreliable)]
                public Package SomeEndpoint1() => null!;

                [Route("correct-route2")]
                [Exchanger]
                public Package SomeEndpoint2() => null!;
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

            public class ControllerWithWrongExchangerEndpoint : IController
            {
                [Route("correct-route")]
                [Exchanger(Reliability.Sequenced)]
                public void WrongExchangerEndpoint()
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
                [Receiver(Reliability.Sequenced)]
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
                [Receiver]
                [Schema("AuthToken", typeof(Guid))]
                [return: Schema("Body", typeof(EndpointsBody))]
                public void SomeEndpoint()
                {
                }
            }
        }
    }
}
