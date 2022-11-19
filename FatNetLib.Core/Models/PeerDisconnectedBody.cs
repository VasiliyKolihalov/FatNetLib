using LiteNetLib;
using NetPeer = Kolyhalov.FatNetLib.Core.Wrappers.NetPeer;

namespace Kolyhalov.FatNetLib.Core.Models
{
    public class PeerDisconnectedBody
    {
        public NetPeer NetPeer { get; set; } = null!;

        public DisconnectInfo DisconnectInfo { get; set; }
    }
}
