using Kolyhalov.FatNetLib.Core.Wrappers;
using LiteNetLib.Utils;

namespace Kolyhalov.FatNetLib.Core.Subscribers
{
    public interface INetworkReceiveEventSubscriber
    {
        public void Handle(INetPeer peer, NetDataReader reader, Reliability reliability);
    }
}
