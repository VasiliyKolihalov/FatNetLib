using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Couriers;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Storages;
using static Kolyhalov.FatNetLib.Core.Constants.RouteConstants.Routes.Events;

namespace Kolyhalov.FatNetLib.Core.Runners
{
    public class InitializersRunner : IInitializersRunner
    {
        private readonly Route _exchangeInitializersRoute = new Route("fat-net-lib/initializers/exchange");
        private readonly IClientCourier _courier;
        private readonly IEndpointsStorage _endpointsStorage;
        private readonly IDependencyContext _context;

        public InitializersRunner(
            IClientCourier courier,
            IEndpointsStorage endpointsStorage,
            IDependencyContext context)
        {
            _courier = courier;
            _endpointsStorage = endpointsStorage;
            _context = context;
        }

        private Guid ServerPeerId => _courier.ServerPeer.Id;

        public async Task RunAsync()
        {
            RegisterInitializersExchanger(_endpointsStorage);
            Package responsePackage = await CallInitializersExchangerAsync();
            IList<Endpoint> initialEndpoints = responsePackage.GetBodyAs<EndpointsBody>().Endpoints;
            RegisterInitializers(initialEndpoints);
            await CallInitializersAsync(initialEndpoints);
            await EmitInitializationFinishedEventAsync();
        }

        private void RegisterInitializersExchanger(IEndpointsStorage endpointsStorage)
        {
            var initializer = new Endpoint(
                _exchangeInitializersRoute,
                EndpointType.Initializer,
                Reliability.ReliableOrdered,
                requestSchemaPatch: new PackageSchema { { nameof(Package.Body), typeof(EndpointsBody) } },
                responseSchemaPatch: new PackageSchema { { nameof(Package.Body), typeof(EndpointsBody) } });
            IDictionary<Guid, IList<Endpoint>> remoteEndpoints = endpointsStorage.RemoteEndpoints;
            if (remoteEndpoints.ContainsKey(ServerPeerId))
            {
                remoteEndpoints[ServerPeerId].Add(initializer);
            }
            else
            {
                remoteEndpoints[ServerPeerId] = new List<Endpoint> { initializer };
            }
        }

        private async Task<Package> CallInitializersExchangerAsync()
        {
            var request = new Package
            {
                Route = _exchangeInitializersRoute,
                Context = _context,
                Body = new EndpointsBody
                {
                    Endpoints = _endpointsStorage
                        .LocalEndpoints
                        .Select(_ => _.Details)
                        .Where(_ => _.Type is EndpointType.Initializer)
                        .ToList()
                },
                Receiver = _courier.ServerPeer
            };
            return (await _courier.SendAsync(request))!;
        }

        private void RegisterInitializers(IEnumerable<Endpoint> initializers)
        {
            foreach (Endpoint initializer in initializers)
            {
                _endpointsStorage.RemoteEndpoints[ServerPeerId].Add(initializer);
            }
        }

        private async Task CallInitializersAsync(IEnumerable<Endpoint> initializers)
        {
            foreach (Endpoint initializer in initializers)
            {
                var package = new Package
                {
                    Route = initializer.Route,
                    Context = _context,
                    Receiver = _courier.ServerPeer
                };
                await _courier.SendAsync(package);
            }
        }

        private async Task EmitInitializationFinishedEventAsync()
        {
            await _courier.EmitEventAsync(new Package
            {
                Route = InitializationFinished,
                Body = _courier.ServerPeer
            });
        }
    }
}
