using LiteNetLib;
using LiteNetLib.Utils;

namespace Kolyhalov.FatNetLib.LiteNetLibWrappers;

public class NetPeer : INetPeer
{
    private readonly LiteNetLib.NetPeer _liteNetLibNetPeer;

    public NetPeer(LiteNetLib.NetPeer netPeer)
    {
        _liteNetLibNetPeer = netPeer;
    }

    public int Id => _liteNetLibNetPeer.Id;

    public void Send(string data, DeliveryMethod deliveryMethod)
    {
        var writer = new NetDataWriter();
        writer.Put(data);
        _liteNetLibNetPeer.Send(writer, deliveryMethod);
    }
}