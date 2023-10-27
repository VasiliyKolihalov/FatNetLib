namespace Kolyhalov.FatNetLib.Core.Models
{
    public enum Reliability
    {
        // Reliable and ordered. Packages won't be dropped, won't be duplicated, will arrive in order
        ReliableOrdered,

        // Reliable. Packages won't be dropped, won't be duplicated, can arrive without order
        ReliableUnordered,

        // Reliable only last package
        // Packages can be dropped (except the last one), won't be duplicated, will arrive in order
        // Cannot be fragmented
        ReliableSequenced,

        // Unreliable. Packages can be dropped, won't be duplicated, will arrive in order
        Sequenced,

        // Unreliable. Packages can be dropped, can be duplicated, can arrive without order.
        Unreliable
    }
}
