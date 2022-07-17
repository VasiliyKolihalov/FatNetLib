using LiteNetLib;

namespace UdpFramework;

public class Exchanger : Attribute
{
    public DeliveryMethod DeliveryMethod { get; }

    public Exchanger(DeliveryMethod deliveryMethod)
    {
        DeliveryMethod = deliveryMethod;
    }
}