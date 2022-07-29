using LiteNetLib;
using LiteNetLib.Utils;

namespace Kolyhalov.UdpFramework;

public class NetPeerShell : INetPeerShell
{
    public readonly NetPeer NetPeer;

    public NetPeerShell(NetPeer netPeer)
    {
        NetPeer = netPeer;
    }

    public int Id => NetPeer.Id;
    
    public void Send(NetDataWriter netDataWriter, DeliveryMethod deliveryMethod)
    {
        NetPeer.Send(netDataWriter, deliveryMethod);
    }
}