using Kolyhalov.FatNetLib.Core.Wrappers;

namespace Kolyhalov.FatNetLib.Core.Models
{
    public class DeliveryEventBody
    {
        public INetPeer Peer { get; set; } = null!;

        public object UserData { get; set; } = null!;
    }
}
