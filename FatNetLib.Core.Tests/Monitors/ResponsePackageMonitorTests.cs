using System;
using System.Threading.Tasks;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Monitors;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Core.Tests.Monitors;

public class ResponsePackageMonitorTests
{
    private readonly Guid _exchangeId = Guid.NewGuid();
    private ResponsePackageMonitor _responsePackageMonitor = null!;

    [SetUp]
    public void SetUp()
    {
        _responsePackageMonitor = new ResponsePackageMonitor(
            new ServerConfiguration
            {
                ExchangeTimeout = TimeSpan.FromMilliseconds(500)
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
    public async Task Pulse_CorrectCase_SetResultForTaskCompletionSource()
    {
        // Arrange
        Task<Package> task = _responsePackageMonitor.WaitAsync(_exchangeId);

        // Act
        _responsePackageMonitor.Pulse(new Package { ExchangeId = _exchangeId });

        // Assert
        Package responsePackage = await task;
        responsePackage.ExchangeId.Should().Be(_exchangeId);
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

    [Test]
    public void Pulse_UnexpectedPackage_Throw()
    {
        // Arrange
        var receivedResponsePackage = new Package { ExchangeId = _exchangeId };

        // Act
        Action act = () => _responsePackageMonitor.Pulse(receivedResponsePackage);

        // Assert
        act.Should().Throw<FatNetLibException>()
            .WithMessage($"There is no waiting for package with {nameof(Package.ExchangeId)} {_exchangeId}");
    }
}
