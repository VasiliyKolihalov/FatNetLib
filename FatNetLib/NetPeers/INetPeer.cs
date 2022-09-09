using LiteNetLib;
using LiteNetLib.Utils;

namespace Kolyhalov.FatNetLib.NetPeers;

public interface INetPeer
{
    public int Id { get; }

    public void Send(NetDataWriter netDataWriter, DeliveryMethod deliveryMethod);
}