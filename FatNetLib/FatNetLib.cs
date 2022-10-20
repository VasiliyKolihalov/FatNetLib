namespace Kolyhalov.FatNetLib;

public class FatNetLib
{
    public IClient Client { get; }
    private readonly NetEventListener _netEventListener;

    public FatNetLib(IClient client, NetEventListener netEventListener)
    {
        Client = client;
        _netEventListener = netEventListener;
    }

    public void Run()
    {
        _netEventListener.Run();
    }

    public void Stop()
    {
        _netEventListener.Stop();
    }
}