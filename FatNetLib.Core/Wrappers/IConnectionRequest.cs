using LiteNetLib.Utils;

namespace Kolyhalov.FatNetLib.Core.Wrappers
{
    public interface IConnectionRequest
    {
        public NetDataReader Data { get; }

        public void Accept();

        public void Reject();
    }
}
