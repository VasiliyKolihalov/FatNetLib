using Kolyhalov.FatNetLib.ResponsePackageMonitors;

namespace Kolyhalov.FatNetLib;

public class FatNetLib
{
    public IFatClient Client { get; }
    public IEndpointRecorder Endpoints { get; }
    private readonly PackageListener _packageListener;

    public FatNetLib(IFatClient client, IEndpointRecorder endpoints, PackageListener packageListener)
    {
        Client = client;
        Endpoints = endpoints;
        _packageListener = packageListener;
    }

    public void Run()
    {
        _packageListener.Run();
    }
    
    public void Stop()
    {
        _packageListener.Stop();
    }
}