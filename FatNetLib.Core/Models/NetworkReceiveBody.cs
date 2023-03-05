using Kolyhalov.FatNetLib.Core.Wrappers;
using LiteNetLib.Utils;

namespace Kolyhalov.FatNetLib.Core.Models
{
    public class NetworkReceiveBody
    {
        public INetPeer Peer { get; set; } = null!;

        public NetDataReader DataReader { get; set; } = null!;

        public Reliability Reliability { get; set; }
    }
}
