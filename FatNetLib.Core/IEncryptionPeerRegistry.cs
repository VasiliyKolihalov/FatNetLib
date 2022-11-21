namespace Kolyhalov.FatNetLib.Core
{
    public interface IEncryptionPeerRegistry
    {
        public void RegisterPeer(int peerId, byte[] key);

        public void UnregisterPeer(int peerId);
    }
}
