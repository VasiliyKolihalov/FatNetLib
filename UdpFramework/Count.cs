namespace Kolyhalov.UdpFramework;

public class Count
{
    public int Value { get; }

    public Count(int value)
    {
        if (value <= 0)
            throw new UdpFrameworkException("Value cannot be zero or bellow");
        
        Value = value;
    }
}