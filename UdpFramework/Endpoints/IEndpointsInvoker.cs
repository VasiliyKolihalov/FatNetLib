namespace Kolyhalov.UdpFramework.Endpoints;

public interface IEndpointsInvoker
{
    [Obsolete("Implement and use InvokeReceiver() or InvokeExchanger()")]
    public Package? InvokeEndpoint(LocalEndpoint endpoint, Package package);

    public void InvokeReceiver(LocalEndpoint endpoint, Package package);

    public void InvokeExchanger(LocalEndpoint endpoint, Package package);
}