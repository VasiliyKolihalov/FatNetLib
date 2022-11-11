namespace Kolyhalov.FatNetLib.Endpoints
{
    public interface IEndpointsInvoker
    {
        public void InvokeReceiver(LocalEndpoint endpoint, Package requestPackage);

        public Package InvokeExchanger(LocalEndpoint endpoint, Package requestPackage);
    }
}
