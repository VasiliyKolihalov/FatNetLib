using LiteNetLib;

namespace UdpFramework;

public class Receiver : Attribute
{
    public DeliveryMethod DeliveryMethod { get; }

    public Receiver(DeliveryMethod deliveryMethod)
    {
        DeliveryMethod = deliveryMethod;
    }
}