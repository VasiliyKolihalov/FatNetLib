namespace Kolyhalov.UdpFramework;

public interface IEndpointsInvoker
{
    public Package? InvokeEndpoint(LocalEndpoint endpoint, Package package);
}