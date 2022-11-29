using LiteNetLib;
using NetPeer = Kolyhalov.FatNetLib.Core.Wrappers.NetPeer;

namespace Kolyhalov.FatNetLib.Core.Models
{
    public class NetworkReceiveBody
    {
        public NetPeer Peer { get; set; } = null!;

        public NetPacketReader PacketReader { get; set; } = null!;

        public Reliability Reliability { get; set; }
    }
}
