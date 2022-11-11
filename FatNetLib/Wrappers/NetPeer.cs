using LiteNetLib.Utils;

namespace Kolyhalov.FatNetLib.Wrappers
{
    public class NetPeer : INetPeer
    {
        private readonly LiteNetLib.NetPeer _liteNetLibNetPeer;

        public NetPeer(LiteNetLib.NetPeer netPeer)
        {
            _liteNetLibNetPeer = netPeer;
        }

        public int Id => _liteNetLibNetPeer.Id;

        public void Send(Package package)
        {
            var writer = new NetDataWriter();
            writer.Put(package.Serialized);
            _liteNetLibNetPeer.Send(writer, DeliveryMethodConverter.ToFatNetLib(package.Reliability!.Value));
        }
    }
}
