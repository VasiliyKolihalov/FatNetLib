using Kolyhalov.FatNetLib.Core.Models;

namespace Kolyhalov.FatNetLib.Core.Components
{
    public interface IEndpointsInvoker
    {
        public void InvokeConsumer(LocalEndpoint endpoint, Package requestPackage);

        public Package InvokeExchanger(LocalEndpoint endpoint, Package requestPackage);
    }
}
