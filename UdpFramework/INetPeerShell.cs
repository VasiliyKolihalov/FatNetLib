using LiteNetLib;
using LiteNetLib.Utils;

namespace Kolyhalov.UdpFramework;

public interface INetPeerShell
{
    public int Id { get; }

    public void Send(NetDataWriter netDataWriter, DeliveryMethod deliveryMethod);
}