﻿using LiteNetLib;
using static System.AttributeTargets;

namespace Kolyhalov.UdpFramework.Attributes;

[AttributeUsage(Method)]
public class Receiver : Attribute
{
    // todo: select and set a default delivery method in order to simplify our api
    // probably, the strongest one
    public DeliveryMethod DeliveryMethod { get; }

    public Receiver(DeliveryMethod deliveryMethod)
    {
        DeliveryMethod = deliveryMethod;
    }
}