using LiteNetLib.Utils;

namespace Kolyhalov.FatNetLib.LiteNetLibWrappers;

public interface IConnectionRequest
{
    public NetDataReader Data { get; }
    
    public void Accept();
    
    public void Reject();
}