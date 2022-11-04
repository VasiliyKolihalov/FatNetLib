using LiteNetLib;
using static System.AttributeTargets;

namespace Kolyhalov.FatNetLib.Attributes;

[AttributeUsage(Method)]
public class ReceiverAttribute : Attribute
{
    public DeliveryMethod DeliveryMethod { get; }

    public ReceiverAttribute(DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
    {
        DeliveryMethod = deliveryMethod;
    }
}
