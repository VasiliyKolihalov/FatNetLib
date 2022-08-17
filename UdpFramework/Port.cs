namespace Kolyhalov.UdpFramework;

public class Port
{
    public int Value { get; }
    private const int MinValidPort = 80;
    private const int MaxValidPort = 65535;

    public Port(int value)
    {
        if (value is < MinValidPort or > MaxValidPort)
            throw new UdpFrameworkException($"Invalid port. Valid range is {MinValidPort} to {MaxValidPort}");

        Value = value;
    }
}