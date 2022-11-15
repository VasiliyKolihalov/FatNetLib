namespace Kolyhalov.FatNetLib.Core
{
    public interface IPeerRegistry
    {
        public void RegisterPeer(int peerId, byte[] key);

        public void UnregisterPeer(int peerId);
    }
}
