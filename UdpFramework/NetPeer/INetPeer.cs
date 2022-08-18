using LiteNetLib;
using LiteNetLib.Utils;

namespace Kolyhalov.UdpFramework.NetPeer;

public interface INetPeer
{
    public int Id { get; }

    public void Send(NetDataWriter netDataWriter, DeliveryMethod deliveryMethod);
}