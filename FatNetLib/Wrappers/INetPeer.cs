using LiteNetLib;

namespace Kolyhalov.FatNetLib.Wrappers;

public interface INetPeer
{
    public int Id { get; }

    public void Send(byte[] data, DeliveryMethod deliveryMethod);
}