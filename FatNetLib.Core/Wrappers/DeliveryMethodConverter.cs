using System;
using Kolyhalov.FatNetLib.Core.Models;
using LiteNetLib;

namespace Kolyhalov.FatNetLib.Core.Wrappers
{
    public static class DeliveryMethodConverter
    {
        public static Reliability FromLiteNetLib(DeliveryMethod deliveryMethod)
        {
            return deliveryMethod switch
            {
                DeliveryMethod.ReliableUnordered => Reliability.ReliableUnordered,
                DeliveryMethod.Sequenced => Reliability.Sequenced,
                DeliveryMethod.ReliableOrdered => Reliability.ReliableOrdered,
                DeliveryMethod.ReliableSequenced => Reliability.ReliableSequenced,
                DeliveryMethod.Unreliable => Reliability.Unreliable,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(deliveryMethod),
                    deliveryMethod,
                    "Unknown DeliveryMethod")
            };
        }

        public static DeliveryMethod ToFatNetLib(Reliability reliability)
        {
            return reliability switch
            {
                Reliability.ReliableUnordered => DeliveryMethod.ReliableUnordered,
                Reliability.Sequenced => DeliveryMethod.Sequenced,
                Reliability.ReliableOrdered => DeliveryMethod.ReliableOrdered,
                Reliability.ReliableSequenced => DeliveryMethod.ReliableSequenced,
                Reliability.Unreliable => DeliveryMethod.Unreliable,
                _ => throw new ArgumentOutOfRangeException(nameof(reliability), reliability, "Unknown Reliability")
            };
        }
    }
}
