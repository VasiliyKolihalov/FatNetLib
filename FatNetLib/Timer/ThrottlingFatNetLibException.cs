namespace Kolyhalov.FatNetLib.Timer;

public class ThrottlingFatNetLibException : FatNetLibException
{
    public TimeSpan ExpectedPeriod { get; }

    public TimeSpan ActualPeriod { get; }

    public ThrottlingFatNetLibException(TimeSpan expectedPeriod, TimeSpan actualPeriod)
        : base($"Throttling detected. Expected period {expectedPeriod}, actual period {actualPeriod}")
    {
        ExpectedPeriod = expectedPeriod;
        ActualPeriod = actualPeriod;
    }
}
