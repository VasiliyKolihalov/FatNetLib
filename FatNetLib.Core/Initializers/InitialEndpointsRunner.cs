using System.Collections.Generic;
using System.Linq;
using Kolyhalov.FatNetLib.Core.Endpoints;
using Kolyhalov.FatNetLib.Core.Microtypes;

namespace Kolyhalov.FatNetLib.Core.Initializers
{
    public class InitialEndpointsRunner : IInitialEndpointsRunner
    {
        private const int ServerPeerId = 0;
        private readonly Route _initialExchangeEndpointsRoute = new Route("fat-net-lib/init-endpoints/exchange");
        private readonly IClient _client;
        private readonly IEndpointsStorage _endpointsStorage;
        private readonly IDependencyContext _context;

        public InitialEndpointsRunner(
            IClient client,
            IEndpointsStorage endpointsStorage,
            IDependencyContext context)
        {
            _client = client;
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
        }

        private void RegisterInitialEndpointsGetter(IEndpointsStorage endpointsStorage)
        {
            var endpoint = new Endpoint(
                _initialExchangeEndpointsRoute,
                EndpointType.Exchanger,
                Reliability.ReliableOrdered,
                isInitial: true,
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
                        .Where(_ => _.IsInitial)
                        .ToList()
                },
                ToPeerId = ServerPeerId
            };
            return _client.SendPackage(request)!;
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
                _client.SendPackage(package);
            }
        }
    }
}
