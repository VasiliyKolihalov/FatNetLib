using System;
using System.Threading;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Timer;
using Moq;
using NUnit.Framework;
using static Moq.Times;

namespace Kolyhalov.FatNetLib.Tests.Timers;

[Timeout(1000)] // 1 second
public class SleepBasedTimerTests
{
    private readonly SleepBasedTimer _timer = new(new Frequency(50)); // period == 20 milliseconds
    private Mock<Action> _action = null!;
    private Mock<ITimerExceptionHandler> _exceptionHandler = null!;

    [SetUp]
    public void SetUp()
    {
        _action = new Mock<Action>();
        _exceptionHandler = new Mock<ITimerExceptionHandler>();
    }

    [TearDown]
    public void TearDown()
    {
        _timer.Stop();
        _timer.Frequency = new Frequency(50);
    }

    [Test]
    public void Start_CorrectTimer_CallActions()
    {
        // Arrange
        var counter = 3;
        _action.Setup(_ => _.Invoke())
            .Callback(() => CallTimerExactlyTimes(_timer, ref counter));

        // Act
        _timer.Start(_action.Object, _exceptionHandler.Object);

        // Assert
        _action.Verify(_ => _.Invoke(), times: Exactly(3));
        _action.VerifyNoOtherCalls();
        _exceptionHandler.VerifyNoOtherCalls();
    }

    [Test]
    public void Start_ThrowingAction_CallExceptionHandler()
    {
        // Arrange
        var counter = 3;

        _action.Setup(_ => _.Invoke())
            .Throws(new ArithmeticException());

        _exceptionHandler.Setup(_ => _.Handle(It.IsAny<Exception>()))
            .Callback(() => CallTimerExactlyTimes(_timer, ref counter));

        // Act
        _timer.Start(_action.Object, _exceptionHandler.Object);

        // Assert
        _action.Verify(_ => _.Invoke(), times: Exactly(3));
        _action.VerifyNoOtherCalls();
        _exceptionHandler.Verify(_ => _.Handle(It.IsAny<ArithmeticException>()), times: Exactly(3));
        _exceptionHandler.VerifyNoOtherCalls();
    }

    [Test]
    public void Start_ThrottlingAction_CallExceptionHandler()
    {
        // Arrange
        _action.Setup(_ => _.Invoke())
            .Callback(() => Thread.Sleep(TimeSpan.FromMilliseconds(100))); // more than timer period == 20 ms

        _exceptionHandler.Setup(_ => _.Handle(It.IsAny<Exception>()))
            .Callback(() => _timer.Stop());

        // Act
        _timer.Start(_action.Object, _exceptionHandler.Object);

        // Assert
        _action.Verify(_ => _.Invoke(), Once);
        _action.VerifyNoOtherCalls();
        _exceptionHandler.Verify(_ => _.Handle(It.IsAny<ThrottlingFatNetLibException>()), Once);
        _exceptionHandler.VerifyNoOtherCalls();
    }

    [Test]
    public void Start_ChangedFrequency_ShouldNotThrottle()
    {
        // Arrange
        _action.Setup(_ => _.Invoke())
            .Callback(() =>
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(50));
                _timer.Stop();
            });

        // Act
        _timer.Frequency = new Frequency(10); // period before == 20 milliseconds, period after == 100 milliseconds

        // Assert
        _timer.Start(_action.Object, _exceptionHandler.Object);
        _action.Verify(_ => _.Invoke(), Once);
        _action.VerifyNoOtherCalls();
        _exceptionHandler.VerifyNoOtherCalls();
    }

    private void CallTimerExactlyTimes(ITimer timer, ref int counter)
    {
        if (--counter <= 0) _timer.Stop();
    }
}
