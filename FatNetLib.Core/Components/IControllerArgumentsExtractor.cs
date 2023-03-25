using Kolyhalov.FatNetLib.Core.Models;

namespace Kolyhalov.FatNetLib.Core.Components
{
    public interface IControllerArgumentsExtractor
    {
        public object?[] ExtractFromPackage(Package package, LocalEndpoint endpoint);
    }
}
