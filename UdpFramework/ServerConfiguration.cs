namespace Kolyhalov.UdpFramework;

public class ServerConfiguration
{
    public Port Port { get; }
    public string ConnectionKey { get; }
    public Framerate Framerate { get; }
    public Count MaxPeers { get;}
    
    public ServerConfiguration(Port port, string connectionKey, Framerate framerate, Count maxPeers)
    {
        if (port == null)
            throw new UdpFrameworkException("Port cannot be null");

        if (framerate == null)
            throw new UdpFrameworkException("Framerate cannot be null");

        if (maxPeers == null)
            throw new UdpFrameworkException("MaxPeersCount cannot be null");
        
        Port = port;
        ConnectionKey = connectionKey;
        Framerate = framerate;
        MaxPeers = maxPeers;
    }
    
}