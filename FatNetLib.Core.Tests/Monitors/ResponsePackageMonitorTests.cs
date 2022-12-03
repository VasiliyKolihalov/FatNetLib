using System;
using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Monitors;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Wrappers;
using Moq;
using NUnit.Framework;
using static Moq.Times;

namespace Kolyhalov.FatNetLib.Core.Tests.Monitors
{
    public class ResponsePackageMonitorTests
    {
        private readonly Guid _exchangeId = Guid.NewGuid();
        private ResponsePackageMonitorStorage _storage = null!;
        private Mock<IMonitor> _monitor = null!;
        private Package _receivedResponsePackage = null!;
        private ResponsePackageMonitor _responsePackageMonitor = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _receivedResponsePackage = new Package { ExchangeId = _exchangeId };
        }

        [SetUp]
        public void SetUp()
        {
            _storage = new ResponsePackageMonitorStorage();
            _monitor = new Mock<IMonitor>();
            _responsePackageMonitor = new ResponsePackageMonitor(
                new Fixture().Create<TimeSpan>(),
                _monitor.Object,
                _storage);
        }

        [Test]
        public void Wait_ResponsePackageReceived_SameResponsePackageReturned()
        {
            // Arrange
            var monitorObjectWaitCapture = new List<object>();
            _monitor
                .Setup(m => m.Wait(Capture.In(monitorObjectWaitCapture), It.IsAny<TimeSpan>()))
                .Callback(() => _storage.ResponsePackages[_exchangeId] = _receivedResponsePackage)
                .Returns(WaitingResult.PulseReceived);

            // Act
            Package actualResponsePackage = _responsePackageMonitor.Wait(_exchangeId);

            // Assert
            _monitor.Verify(m => m.Wait(It.IsAny<object>(), It.IsAny<TimeSpan>()), Once);
            _storage.MonitorsObjects.Should().BeEmpty();
            _storage.ResponsePackages.Should().BeEmpty();
            actualResponsePackage.Should().Be(_receivedResponsePackage);
        }

        [Test]
        public void Wait_ExchangeIdIsAlreadyAwaitedByOtherThread_Throw()
        {
            // Arrange
            _storage.MonitorsObjects[_exchangeId] = _receivedResponsePackage;

            // Act
            Action act = () => _responsePackageMonitor.Wait(_exchangeId);

            // Assert
            act.Should()
                .Throw<FatNetLibException>()
                .WithMessage($"ExchangeId {_exchangeId} is already being waited by someone");
        }

        [Test]
        public void Wait_ResponsePackageNotReceived_Throw()
        {
            // Arrange
            _monitor.Setup(m => m.Wait(It.IsAny<object>(), It.IsAny<TimeSpan>()))
                .Returns(WaitingResult.InterruptedByTimeout);

            // Act
            Action act = () => _responsePackageMonitor.Wait(_exchangeId);

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage($"ExchangeId {_exchangeId} response timeout exceeded");
        }

        [Test]
        public void Pulse_ResponsePackageReceived_WaitingThreadIsNotified()
        {
            // Arrange
            _storage.MonitorsObjects[_exchangeId] = new object();

            // Act
            _responsePackageMonitor.Pulse(_receivedResponsePackage);

            // Assert
            _monitor.Verify(m => m.Pulse(It.IsAny<object>()), Once);
            _storage.ResponsePackages.Should().Contain(_exchangeId, _receivedResponsePackage);
            _storage.MonitorsObjects.Should().BeEmpty();
        }

        [Test]
        public void Pulse_ResponsePackageWithoutExchangeId_Throw()
        {
            // Arrange
            var receivedResponsePackage = new Package();

            // Act
            Action act = () => _responsePackageMonitor.Pulse(receivedResponsePackage);

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage("Field ExchangeId was not present in the package");
        }

        [Test]
        public void Pulse_NoWaitingThreadsForExchangeId_Skip()
        {
            // Act
            _responsePackageMonitor.Pulse(_receivedResponsePackage);

            // Assert
            _monitor.Verify(m => m.Pulse(It.IsAny<object>()), Never);
            _storage.ResponsePackages.Should().BeEmpty();
        }
    }
}
