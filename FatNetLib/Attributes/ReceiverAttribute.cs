using static System.AttributeTargets;

namespace Kolyhalov.FatNetLib.Attributes;

[AttributeUsage(Method)]
public class ReceiverAttribute : Attribute
{
    public Reliability Reliability { get; }

    public ReceiverAttribute(Reliability reliability = Reliability.ReliableOrdered)
    {
        Reliability = reliability;
    }
}
