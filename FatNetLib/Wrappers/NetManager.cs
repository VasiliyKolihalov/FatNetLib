namespace Kolyhalov.FatNetLib.Wrappers;

public class NetManager : INetManager
{
    private readonly LiteNetLib.NetManager _netManager;

    public int ConnectedPeersCount => _netManager.ConnectedPeersCount;

    public NetManager(LiteNetLib.NetManager netManager)
    {
        _netManager = netManager;
    }

    public void Connect(string address, int port, string key)
    {
        _netManager.Connect(address, port, key);
    }

    public void Start()
    {
        _netManager.Start();
    }

    public void Start(int port)
    {
        _netManager.Start(port);
    }

    public void Stop()
    {
        _netManager.Stop();
    }

    public void PollEvents()
    {
        _netManager.PollEvents();
    }
}
