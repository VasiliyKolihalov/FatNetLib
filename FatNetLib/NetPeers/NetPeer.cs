using LiteNetLib;
using LiteNetLib.Utils;

namespace Kolyhalov.FatNetLib.NetPeers;

public class NetPeer : INetPeer
{
    private readonly LiteNetLib.NetPeer _liteNetLibNetPeer;

    public NetPeer(LiteNetLib.NetPeer netPeer)
    {
        _liteNetLibNetPeer = netPeer;
    }

    public int Id => _liteNetLibNetPeer.Id;
    
    public void Send(NetDataWriter netDataWriter, DeliveryMethod deliveryMethod)
    {
        _liteNetLibNetPeer.Send(netDataWriter, deliveryMethod);
    }
}