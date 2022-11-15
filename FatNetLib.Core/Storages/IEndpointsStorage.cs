using System.Collections.Generic;

namespace Kolyhalov.FatNetLib.Core.Storages
{
    // Todo: think about concurrency here
    public interface IEndpointsStorage
    {
        public IList<LocalEndpoint> LocalEndpoints { get; }

        public IDictionary<int, IList<Endpoint>> RemoteEndpoints { get; }
    }
}
