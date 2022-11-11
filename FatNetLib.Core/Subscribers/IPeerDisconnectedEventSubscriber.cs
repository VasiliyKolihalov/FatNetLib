using Kolyhalov.FatNetLib.Core.Wrappers;
using LiteNetLib;

namespace Kolyhalov.FatNetLib.Core.Subscribers
{
    public interface IPeerDisconnectedEventSubscriber
    {
        public void Handle(INetPeer peer, DisconnectInfo info);
    }
}
