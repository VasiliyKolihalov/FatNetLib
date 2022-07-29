namespace Kolyhalov.UdpFramework;

public class ServerConfiguration
{
    public int Port { get; }
    public string ConnectionKey { get; }
    public int Framerate { get; }
    public int MaxPeersCount { get;}
    
    public ServerConfiguration(int port, string connectionKey, int framerate, int maxPeersCount)
    {
        Port = port;
        ConnectionKey = connectionKey;
        Framerate = framerate;
        MaxPeersCount = maxPeersCount;
    }
    
}