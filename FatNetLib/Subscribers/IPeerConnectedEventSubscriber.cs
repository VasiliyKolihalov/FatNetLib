using Kolyhalov.FatNetLib.Wrappers;

namespace Kolyhalov.FatNetLib.Subscribers;

public interface IPeerConnectedEventSubscriber
{
    public void Handle(INetPeer peer);
}
