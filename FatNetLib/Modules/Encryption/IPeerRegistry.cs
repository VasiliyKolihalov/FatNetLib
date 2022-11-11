namespace Kolyhalov.FatNetLib.Modules.Encryption
{
    public interface IPeerRegistry
    {
        public void RegisterPeer(int peerId, byte[] key);

        public void UnregisterPeer(int peerId);
    }
}
