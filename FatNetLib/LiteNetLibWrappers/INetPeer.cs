using LiteNetLib;

namespace Kolyhalov.FatNetLib.LiteNetLibWrappers;

public interface INetPeer
{
    public int Id { get; }

    public void Send(string data, DeliveryMethod deliveryMethod);
}