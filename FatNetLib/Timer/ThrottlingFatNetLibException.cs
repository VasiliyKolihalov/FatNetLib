namespace Kolyhalov.FatNetLib.Timer;

public class ThrottlingFatNetLibException : FatNetLibException
{
    public readonly TimeSpan ExpectedPeriod;
    public readonly TimeSpan ActualPeriod;

    public ThrottlingFatNetLibException(TimeSpan expectedPeriod, TimeSpan actualPeriod)
        : base($"Throttling detected. Expected period {expectedPeriod}, actual period {actualPeriod}")
    {
        ExpectedPeriod = expectedPeriod;
        ActualPeriod = actualPeriod;
    }
}
