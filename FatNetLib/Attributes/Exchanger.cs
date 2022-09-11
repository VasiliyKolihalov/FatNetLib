using LiteNetLib;
using static System.AttributeTargets;

namespace Kolyhalov.FatNetLib.Attributes;

[AttributeUsage(Method)]
public class Exchanger : Attribute
{
    public DeliveryMethod DeliveryMethod { get; }

    public Exchanger(DeliveryMethod deliveryMethod)
    {
        DeliveryMethod = deliveryMethod;
    }
}