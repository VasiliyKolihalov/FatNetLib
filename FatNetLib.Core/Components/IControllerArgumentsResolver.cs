using Kolyhalov.FatNetLib.Core.Models;

namespace Kolyhalov.FatNetLib.Core.Components
{
    public interface IControllerArgumentsResolver
    {
        public object?[] GetEndpointArguments(LocalEndpoint endpoint, Package package);
    }
}
