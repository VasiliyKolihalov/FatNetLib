using Kolyhalov.FatNetLib.Core.Wrappers;

namespace Kolyhalov.FatNetLib.Core.Components
{
    public interface IEncryptionPeerRegistry
    {
        public void RegisterPeer(INetPeer peer, byte[] key);

        public void UnregisterPeer(INetPeer peer);
    }
}
