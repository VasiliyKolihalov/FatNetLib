using System;
using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Models;

namespace Kolyhalov.FatNetLib.Core.Storages
{
    // Todo: think about concurrency here
    public interface IEndpointsStorage
    {
        public IList<LocalEndpoint> LocalEndpoints { get; }

        public IDictionary<Guid, IList<Endpoint>> RemoteEndpoints { get; }
    }
}
