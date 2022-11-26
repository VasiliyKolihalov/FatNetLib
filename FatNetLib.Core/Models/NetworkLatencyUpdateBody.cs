using Kolyhalov.FatNetLib.Core.Wrappers;

namespace Kolyhalov.FatNetLib.Core.Models
{
    public class NetworkLatencyUpdateBody
    {
        public INetPeer Peer { get; set; } = null!;

        public int Latency { get; set; }
    }
}
