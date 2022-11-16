using Kolyhalov.FatNetLib.Core.Models;

namespace Kolyhalov.FatNetLib.Core
{
    public interface IEndpointsInvoker
    {
        public void InvokeReceiver(LocalEndpoint endpoint, Package requestPackage);

        public Package InvokeExchanger(LocalEndpoint endpoint, Package requestPackage);
    }
}
