namespace Kolyhalov.FatNetLib.Core.Wrappers
{
    public interface INetPeer
    {
        public int Id { get; }

        public void Send(Package package);
    }
}
