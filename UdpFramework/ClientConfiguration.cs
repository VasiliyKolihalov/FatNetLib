namespace UdpFramework;

public class ClientConfiguration
{
    public string Address { get; }
    public int Port { get; }
    public string ConnectionKey { get; }
    public int Framerate { get; }
    
    public ClientConfiguration(string address, int port, string connectionKey, int framerate)
    {
        Address = address;
        Port = port;
        ConnectionKey = connectionKey;
        Framerate = framerate;
    }
}