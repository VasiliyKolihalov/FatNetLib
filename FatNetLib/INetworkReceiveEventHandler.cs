using Kolyhalov.FatNetLib.NetPeers;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Kolyhalov.FatNetLib;

public interface INetworkReceiveEventHandler
{
    public void Handle(INetPeer peer, NetDataReader reader, DeliveryMethod deliveryMethod);
}