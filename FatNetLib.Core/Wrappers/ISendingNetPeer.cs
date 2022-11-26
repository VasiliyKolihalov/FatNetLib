using Kolyhalov.FatNetLib.Core.Models;

namespace Kolyhalov.FatNetLib.Core.Wrappers
{
    public interface ISendingNetPeer : INetPeer
    {
        public void Send(Package package);
    }
}
