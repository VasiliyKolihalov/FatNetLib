namespace Kolyhalov.UdpFramework.Microtypes;

public class Count
{
    public int Value { get; }

    public Count(int value)
    {
        if (value < 0)
            throw new UdpFrameworkException("Value cannot be bellow zero");
        
        Value = value;
    }
}