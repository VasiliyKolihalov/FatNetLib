using LiteNetLib.Utils;

namespace Kolyhalov.FatNetLib.Wrappers;

public class ConnectionRequest : IConnectionRequest
{
    private readonly LiteNetLib.ConnectionRequest _connectionRequest;
    
    public ConnectionRequest(LiteNetLib.ConnectionRequest connectionRequest)
    {
        _connectionRequest = connectionRequest;
    }

    public NetDataReader Data => _connectionRequest.Data;

    public void Accept()
    {
        _connectionRequest.Accept();
    }

    public void Reject()
    {
        _connectionRequest.Reject();
    }
}