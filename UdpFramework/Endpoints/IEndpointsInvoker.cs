namespace Kolyhalov.UdpFramework.Endpoints;

public interface IEndpointsInvoker
{
    public Package? InvokeEndpoint(LocalEndpoint endpoint, Package package);
}