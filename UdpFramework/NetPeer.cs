using LiteNetLib;
using LiteNetLib.Utils;

namespace Kolyhalov.UdpFramework;

public class NetPeer : INetPeerShell
{
    public readonly LiteNetLib.NetPeer LiteNetLibNetPeer;

    public NetPeer(LiteNetLib.NetPeer netPeer)
    {
        LiteNetLibNetPeer = netPeer;
    }

    public int Id => LiteNetLibNetPeer.Id;
    
    public void Send(NetDataWriter netDataWriter, DeliveryMethod deliveryMethod)
    {
        LiteNetLibNetPeer.Send(netDataWriter, deliveryMethod);
    }
}