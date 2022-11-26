using Kolyhalov.FatNetLib.Core.Wrappers;

namespace Kolyhalov.FatNetLib.Core.Models
{
    public class DeliveryEventBody
    {
        public INetPeer NetPeer { get; set; } = null!;

        public object UserData { get; set; } = null!;
    }
}
