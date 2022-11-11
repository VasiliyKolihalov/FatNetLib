using Kolyhalov.FatNetLib.Core.Wrappers;

namespace Kolyhalov.FatNetLib.Core.Subscribers
{
    public interface IPeerConnectedEventSubscriber
    {
        public void Handle(INetPeer peer);
    }
}
