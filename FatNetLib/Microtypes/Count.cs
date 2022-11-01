namespace Kolyhalov.FatNetLib.Microtypes;

public class Count
{
    public int Value { get; }

    public Count(int value)
    {
        if (value < 0)
            throw new FatNetLibException("Value cannot be bellow zero");

        Value = value;
    }
}