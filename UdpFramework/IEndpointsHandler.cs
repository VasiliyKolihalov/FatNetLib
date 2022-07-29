namespace Kolyhalov.UdpFramework;

public interface IEndpointsHandler
{
    public Package? HandleEndpoint(LocalEndpoint endpoint, Package package);
}