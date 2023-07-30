using System.Threading.Tasks;
using Kolyhalov.FatNetLib.Core.Models;

namespace Kolyhalov.FatNetLib.Core.Components
{
    public interface IEndpointsInvoker
    {
        public Task InvokeConsumerAsync(LocalEndpoint endpoint, Package requestPackage);

        public Task<Package> InvokeExchangerAsync(LocalEndpoint endpoint, Package requestPackage);
    }
}
