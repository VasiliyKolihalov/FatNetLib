using Kolyhalov.FatNetLib.Wrappers;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Kolyhalov.FatNetLib.Subscribers;

public interface INetworkReceiveEventSubscriber
{
    public void Handle(INetPeer peer, NetDataReader reader, DeliveryMethod deliveryMethod);
}
