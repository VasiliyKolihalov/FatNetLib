using LiteNetLib;

namespace Kolyhalov.FatNetLib;

public static class ReliabilityConverter
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
            _ => throw new ArgumentOutOfRangeException(nameof(deliveryMethod), deliveryMethod, "Unknown Reliability")
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
