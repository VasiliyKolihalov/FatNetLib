namespace Kolyhalov.UdpFramework;

public class ClientConfiguration
{
    public string Address { get; }
    public Port Port { get; }
    public string ConnectionKey { get; }
    public Framerate Framerate { get; }
    
    public ClientConfiguration(string address, Port port, string connectionKey, Framerate framerate)
    {
        if (address == null)
            throw new UdpFrameworkException("Address cannot be null");

        if (port == null)
            throw new UdpFrameworkException("Port cannot be null");

        if (framerate == null)
            throw new UdpFrameworkException("Framerate cannot be null");
        
        Address = address;
        Port = port;
        ConnectionKey = connectionKey;
        Framerate = framerate;
    }
}