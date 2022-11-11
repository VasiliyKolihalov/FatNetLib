namespace Kolyhalov.FatNetLib.Core
{
    public enum Reliability
    {
        // Reliable. Packets won't be dropped, won't be duplicated, can arrive without order
        ReliableUnordered,

        // Unreliable. Packets can be dropped, won't be duplicated, will arrive in order
        Sequenced,

        // Reliable and ordered. Packets won't be dropped, won't be duplicated, will arrive in order
        ReliableOrdered,

        // Reliable only last packet
        // Packets can be dropped (except the last one), won't be duplicated, will arrive in order
        // Cannot be fragmented
        ReliableSequenced,

        // Unreliable. Packets can be dropped, can be duplicated, can arrive without order.
        Unreliable
    }
}
