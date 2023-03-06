using Kolyhalov.FatNetLib.Core.Wrappers;
using LiteNetLib;

namespace Kolyhalov.FatNetLib.Core.Models
{
    public class PeerDisconnectedBody
    {
        public INetPeer Peer { get; set; } = null!;

        public DisconnectInfo DisconnectInfo { get; set; }
    }
}
