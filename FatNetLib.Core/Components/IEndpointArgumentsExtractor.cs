using Kolyhalov.FatNetLib.Core.Models;

namespace Kolyhalov.FatNetLib.Core.Components
{
    public interface IEndpointArgumentsExtractor
    {
        public object?[] ExtractFromPackage(Package package, LocalEndpoint endpoint);
    }
}
