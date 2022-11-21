using System.Collections.Generic;
using System.Linq;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Storages;

namespace Kolyhalov.FatNetLib.Core.Runners
{
    public class InitialEndpointsRunner : IInitialEndpointsRunner
    {
        private const int ServerPeerId = 0;
        private readonly Route _initialExchangeEndpointsRoute = new Route("fat-net-lib/init-endpoints/exchange");
        private readonly ICourier _courier;
        private readonly IEndpointsStorage _endpointsStorage;
        private readonly IDependencyContext _context;

        public InitialEndpointsRunner(
            ICourier courier,
            IEndpointsStorage endpointsStorage,
            IDependencyContext context)
        {
            _courier = courier;
            _endpointsStorage = endpointsStorage;
            _context = context;
        }

        public void Run()
        {
            RegisterInitialEndpointsGetter(_endpointsStorage);
            Package responsePackage = CallInitialEndpointsGetter();
            IList<Endpoint> initialEndpoints = responsePackage.GetBodyAs<EndpointsBody>().Endpoints;
            RegisterInitialEndpoints(initialEndpoints);
            CallInitialEndpoints(initialEndpoints);
            // Todo: emit initialization finished event
        }

        private void RegisterInitialEndpointsGetter(IEndpointsStorage endpointsStorage)
        {
            var endpoint = new Endpoint(
                _initialExchangeEndpointsRoute,
                EndpointType.Initial,
                Reliability.ReliableOrdered,
                requestSchemaPatch: new PackageSchema { { nameof(Package.Body), typeof(EndpointsBody) } },
                responseSchemaPatch: new PackageSchema { { nameof(Package.Body), typeof(EndpointsBody) } });
            IDictionary<int, IList<Endpoint>> remoteEndpoints = endpointsStorage.RemoteEndpoints;
            if (remoteEndpoints.ContainsKey(ServerPeerId))
            {
                remoteEndpoints[ServerPeerId].Add(endpoint);
            }
            else
            {
                remoteEndpoints[ServerPeerId] = new List<Endpoint> { endpoint };
            }
        }

        private Package CallInitialEndpointsGetter()
        {
            var request = new Package
            {
                Route = _initialExchangeEndpointsRoute,
                Context = _context,
                Body = new EndpointsBody
                {
                    Endpoints = _endpointsStorage
                        .LocalEndpoints
                        .Select(_ => _.EndpointData)
                        .Where(_ => _.EndpointType is EndpointType.Initial)
                        .ToList()
                },
                ToPeerId = ServerPeerId
            };
            return _courier.Send(request)!;
        }

        private void RegisterInitialEndpoints(IEnumerable<Endpoint> endpoints)
        {
            foreach (Endpoint endpoint in endpoints)
            {
                _endpointsStorage.RemoteEndpoints[ServerPeerId].Add(endpoint);
            }
        }

        private void CallInitialEndpoints(IEnumerable<Endpoint> endpoints)
        {
            foreach (Endpoint endpoint in endpoints)
            {
                var package = new Package
                {
                    Route = endpoint.Route,
                    Context = _context,
                    ToPeerId = ServerPeerId
                };
                _courier.Send(package);
            }
        }
    }
}
