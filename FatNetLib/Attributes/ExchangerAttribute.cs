using LiteNetLib;
using static System.AttributeTargets;

namespace Kolyhalov.FatNetLib.Attributes;

[AttributeUsage(Method)]
public class ExchangerAttribute : Attribute
{
    public DeliveryMethod DeliveryMethod { get; }

    public ExchangerAttribute(DeliveryMethod deliveryMethod = DeliveryMethod.ReliableOrdered)
    {
        DeliveryMethod = deliveryMethod;
    }
}