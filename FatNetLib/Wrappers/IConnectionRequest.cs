using LiteNetLib.Utils;

namespace Kolyhalov.FatNetLib.Wrappers;

public interface IConnectionRequest
{
    public NetDataReader Data { get; }

    public void Accept();

    public void Reject();
}
