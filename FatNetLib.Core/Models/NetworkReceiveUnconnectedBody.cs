using System.Net;
using LiteNetLib;

namespace Kolyhalov.FatNetLib.Core.Models
{
    public class NetworkReceiveUnconnectedBody
    {
        public IPEndPoint IPEndPoint { get; set; } = null!;

        public NetPacketReader NetPacketReader { get; set; } = null!;

        public UnconnectedMessageType UnconnectedMessageType { get; set; }
    }
}
