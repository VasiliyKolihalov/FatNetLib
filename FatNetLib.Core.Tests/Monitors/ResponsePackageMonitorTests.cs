/*
using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Monitors;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Wrappers;
using Moq;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Core.Tests.Monitors;

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
            new ServerConfiguration
            {
                ExchangeTimeout = new Fixture().Create<TimeSpan>()
            });
    }

    [Test]
    public async Task WaitAsync_ResponsePackageNotReceived_Throw()
    {
        // Act
        Func<Task> act = async () => await _responsePackageMonitor.WaitAsync(_exchangeId);

        // Assert
        await act.Should().ThrowAsync<FatNetLibException>()
            .WithMessage($"ExchangeId {_exchangeId} response timeout exceeded");
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
            .WithMessage("ExchangeId is null, which is not allowed");
    }
}
*/
