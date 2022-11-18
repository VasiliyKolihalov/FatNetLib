using System;
using System.Collections.Generic;
using System.Linq;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Storages;

namespace Kolyhalov.FatNetLib.Core
{
    public class EventsEmitter : IEventsEmitter
    {
        private readonly IEndpointsStorage _endpointsStorage;
        private readonly IEndpointsInvoker _endpointsInvoker;

        public EventsEmitter(IEndpointsStorage endpointsStorage, IEndpointsInvoker endpointsInvoker)
        {
            _endpointsStorage = endpointsStorage;
            _endpointsInvoker = endpointsInvoker;
        }

        public void Emit(Package package)
        {
            if (package is null) throw new ArgumentNullException(nameof(package));
            if (package.Route is null) throw new ArgumentNullException(nameof(package.Route));

            IEnumerable<LocalEndpoint> endpoints = _endpointsStorage.LocalEndpoints
                .Where(_ => _.EndpointData.Route.Equals(package.Route)).ToList();

            if (!endpoints.Any())
                throw new FatNetLibException($"No event-endpoints registered with route {package.Route}");

            foreach (LocalEndpoint endpoint in endpoints)
            {
                _endpointsInvoker.InvokeReceiver(endpoint, package);
            }
        }
    }
}
