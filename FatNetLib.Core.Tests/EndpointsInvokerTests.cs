﻿using System;
using System.Reflection;
using AutoFixture.NUnit3;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Delegates;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Models;
using Moq;
using NUnit.Framework;
using static Moq.Times;

namespace Kolyhalov.FatNetLib.Core.Tests
{
    public class EndpointsInvokerTests
    {
        private readonly EndpointsInvoker _endpointsInvoker = new EndpointsInvoker();

        [Test, AutoData]
        public void InvokeReceiver_CorrectCase_InvokeAction(Reliability reliability)
        {
            // Arrange
            var receiverAction = new Mock<ReceiverAction>();
            LocalEndpoint endpoint = ALocalEndpoint(EndpointType.Receiver, receiverAction);
            var requestPackage = new Package();

            // Act
            _endpointsInvoker.InvokeReceiver(endpoint, requestPackage);

            // Assert
            receiverAction.Verify(_ => _.Invoke(requestPackage), Once);
        }

        [Test, AutoData]
        public void InvokeExchanger_CorrectCase_InvokeDelegateReturnPackage(Reliability reliability)
        {
            // Arrange
            var exchangerAction = new Mock<ExchangerAction>();
            var responsePackage = new Package();
            exchangerAction.Setup(_ => _.Invoke(It.IsAny<Package>()))
                .Returns(responsePackage);
            LocalEndpoint endpoint = ALocalEndpoint(EndpointType.Exchanger, exchangerAction);
            var requestPackage = new Package();

            // Act
            Package actualResponsePackage = _endpointsInvoker.InvokeExchanger(endpoint, requestPackage);

            // Assert
            exchangerAction.Verify(_ => _.Invoke(requestPackage), Once);
            actualResponsePackage.Should().Be(responsePackage);
        }

        [Test]
        public void InvokeExchanger_EndpointReturnsNull_Throw()
        {
            // Arrange
            var exchangerAction = new Mock<ExchangerAction>();
            exchangerAction.Setup(_ => _.Invoke(It.IsAny<Package>()))
                .Returns((Package)null!);
            LocalEndpoint endpoint = ALocalEndpoint(EndpointType.Exchanger, exchangerAction);

            // Act
            Action act = () => _endpointsInvoker.InvokeExchanger(endpoint, requestPackage: new Package());

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage("Exchanger cannot return null");
        }

        [Test]
        public void InvokeExchanger_ResponsePackageWithAnotherRoute_Throw()
        {
            // Arrange
            var exchangerAction = new Mock<ExchangerAction>();
            var responsePackage = new Package { Route = new Route("another/route") };
            exchangerAction.Setup(_ => _.Invoke(It.IsAny<Package>()))
                .Returns(responsePackage);
            LocalEndpoint endpoint = ALocalEndpoint(EndpointType.Exchanger, exchangerAction);
            var requestPackage = new Package();

            // Act
            Action act = () => _endpointsInvoker.InvokeExchanger(endpoint, requestPackage);

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage("Pointing response packages to another route is not allowed");
        }

        [Test]
        public void InvokeExchanger_ResponsePackageWithAnotherExchangeId_Throw()
        {
            // Arrange
            var exchangerAction = new Mock<ExchangerAction>();
            var responsePackage = new Package { ExchangeId = Guid.NewGuid() };
            exchangerAction.Setup(_ => _.Invoke(It.IsAny<Package>()))
                .Returns(responsePackage);
            LocalEndpoint endpoint = ALocalEndpoint(EndpointType.Exchanger, exchangerAction);
            var requestPackage = new Package();

            // Act
            Action act = () => _endpointsInvoker.InvokeExchanger(endpoint, requestPackage);

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage("Changing response exchangeId to another is not allowed");
        }

        [Test]
        public void InvokeEndpoint_EndpointThrow_Throw()
        {
            // Arrange
            var exchangerAction = new Mock<ReceiverAction>();
            exchangerAction.Setup(_ => _.Invoke(It.IsAny<Package>()))
                .Throws(new ArithmeticException("bad calculation"));
            LocalEndpoint endpoint = ALocalEndpoint(EndpointType.Receiver, exchangerAction);
            var requestPackage = new Package();

            // Act
            Action act = () => _endpointsInvoker.InvokeReceiver(endpoint, requestPackage);

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage("Endpoint invocation failed")
                .WithInnerException(typeof(TargetInvocationException))
                .WithInnerException(typeof(ArithmeticException))
                .WithMessage("bad calculation");
        }

        private static LocalEndpoint ALocalEndpoint(EndpointType endpointType, IMock<Delegate> action)
        {
            return new LocalEndpoint(
                new Endpoint(
                    new Route("test/route"),
                    endpointType,
                    Reliability.Sequenced,
                    requestSchemaPatch: new PackageSchema(),
                    responseSchemaPatch: new PackageSchema()),
                action.Object);
        }
    }
}
