namespace Kolyhalov.UdpFramework.Microtypes;

public class Framerate
{
    public int Value { get; }

    public Framerate(int value)
    {
        if (value < 0)
            throw new UdpFrameworkException("Framerate cannot be below zero");

        Value = value;
    }
}