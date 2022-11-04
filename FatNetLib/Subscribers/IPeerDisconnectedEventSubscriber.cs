using Kolyhalov.FatNetLib.Wrappers;
using LiteNetLib;

namespace Kolyhalov.FatNetLib.Subscribers;

public interface IPeerDisconnectedEventSubscriber
{
    public void Handle(INetPeer peer, DisconnectInfo info);
}
