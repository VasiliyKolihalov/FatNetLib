namespace Kolyhalov.UdpFramework;

public class Port
{
    public int UdpPort { get; }
    private const int MinValidPort = 1024;
    private const int MaxValidPort = 49151;

    public Port(int port)
    {
        if (port is < MinValidPort or > MaxValidPort)
            throw new UdpFrameworkException("invalid port");
                
        UdpPort = port;
    }
}