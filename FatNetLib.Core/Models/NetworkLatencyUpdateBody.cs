using Kolyhalov.FatNetLib.Core.Wrappers;

namespace Kolyhalov.FatNetLib.Core.Models
{
    public class NetworkLatencyUpdateBody
    {
        public ISendingNetPeer Peer { get; set; } = null!;

        public int Latency { get; set; }
    }
}
