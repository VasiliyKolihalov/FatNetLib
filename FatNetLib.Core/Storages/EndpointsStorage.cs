using System.Collections.Generic;

namespace Kolyhalov.FatNetLib.Core.Storages
{
    public class EndpointsStorage : IEndpointsStorage
    {
        public IList<LocalEndpoint> LocalEndpoints { get; } = new List<LocalEndpoint>();

        public IDictionary<int, IList<Endpoint>> RemoteEndpoints { get; } = new Dictionary<int, IList<Endpoint>>();
    }
}
