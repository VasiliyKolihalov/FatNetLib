
namespace Kolyhalov.FatNetLib;

public class FatNetLib
{
    public IClient Client { get; }
    public IEndpointRecorder Endpoints { get; }
    private readonly NetEventListener _netEventListener;

    public FatNetLib(IClient client, IEndpointRecorder endpoints, NetEventListener netEventListener)
    {
        Client = client;
        Endpoints = endpoints;
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