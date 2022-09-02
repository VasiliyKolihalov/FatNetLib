using System;
using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using Kolyhalov.UdpFramework;
using Kolyhalov.UdpFramework.Configurations;
using Kolyhalov.UdpFramework.ResponsePackageMonitors;
using Moq;
using NUnit.Framework;
using static Moq.Times;

namespace UdpFrameworkTests;

public class ResponsePackageMonitorTests
{
    private ResponsePackageMonitorStorage _storage = null!;
    private Mock<IMonitor> _monitor = null!;
    private readonly Guid _exchangeId = Guid.NewGuid();
    private Package _receivedResponsePackage = null!;
    private ResponsePackageMonitor _responsePackageMonitor = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _receivedResponsePackage = new Fixture()
            .Build<Package>()
            .With(package => package.ExchangeId, _exchangeId)
            .Create();
    }

    [SetUp]
    public void SetUp()
    {
        _storage = new ResponsePackageMonitorStorage();
        _monitor = new Mock<IMonitor>();
        _responsePackageMonitor = new ResponsePackageMonitor(_monitor.Object, new Fixture().Create<TimeSpan>(), 
            _storage);
    }


    [Test]
    public void Wait_ResponsePackageReceived_SameResponsePackageReturned()
    {
        // Arrange
        List<object> monitorObjectWaitCapture = new();
        _monitor
            .Setup(m => m.Wait(Capture.In(monitorObjectWaitCapture), It.IsAny<TimeSpan>()))
            .Callback(() => _storage.ResponsePackages[_exchangeId] = _receivedResponsePackage)
            .Returns(WaitingResult.PulseReceived);

        // Act
        var actualResponsePackage = _responsePackageMonitor.Wait(_exchangeId);

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
            .Throw<UdpFrameworkException>()
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
        act.Should().Throw<UdpFrameworkException>()
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
        var receivedResponsePackage = new Fixture()
            .Build<Package>()
            .With(package => package.ExchangeId, (Guid?) null)
            .Create();
        
        // Act
        Action act = () => _responsePackageMonitor.Pulse(receivedResponsePackage);

        // Assert
        act.Should().Throw<UdpFrameworkException>()
            .WithMessage("Response package must have an exchangeId");
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