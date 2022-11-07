using Kolyhalov.FatNetLib.Wrappers;
using LiteNetLib.Utils;

namespace Kolyhalov.FatNetLib.Subscribers;

public interface INetworkReceiveEventSubscriber
{
    public void Handle(INetPeer peer, NetDataReader reader, Reliability reliability);
}
