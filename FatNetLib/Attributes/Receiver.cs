using LiteNetLib;
using static System.AttributeTargets;

namespace Kolyhalov.FatNetLib.Attributes;

[AttributeUsage(Method)]
public class Receiver : Attribute
{
    public DeliveryMethod DeliveryMethod { get; }

    public Receiver(DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
    {
        DeliveryMethod = deliveryMethod;
    }
}