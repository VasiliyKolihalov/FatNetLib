using System.Collections.Generic;
using System.Linq;
using Kolyhalov.FatNetLib.Core.Attributes;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Utils;

namespace Kolyhalov.FatNetLib.Core.Controllers.Server
{
    [Route("fat-net-lib")]
    public class ExchangeInitializersController : IController
    {
        private readonly IEndpointsStorage _endpointsStorage;

        public ExchangeInitializersController(IEndpointsStorage endpointsStorage)
        {
            _endpointsStorage = endpointsStorage;
        }

        [Initializer]
        [Route("initializers/exchange")]
        [Schema(key: nameof(Package.Body), type: typeof(EndpointsBody))]
        [return: Schema(key: nameof(Package.Body), type: typeof(EndpointsBody))]
        public Package ExchangeInitializers(Package package)
        {
            SaveClientInitializers(package);
            return PackLocalInitializers();
        }

        private void SaveClientInitializers(Package package)
        {
            int fromPeerId = package.FromPeer!.Id;
            IList<Endpoint> endpoints = package.GetBodyAs<EndpointsBody>().Endpoints;
            IDictionary<int, IList<Endpoint>> remoteEndpoints = _endpointsStorage.RemoteEndpoints;
            _endpointsStorage.RemoteEndpoints[fromPeerId] = remoteEndpoints.ContainsKey(fromPeerId)
                ? remoteEndpoints[fromPeerId].Concat(endpoints).ToList()
                : endpoints;
        }

        private Package PackLocalInitializers()
        {
            var currentRoute = new Route("fat-net-lib/initializers/exchange");
            return new Package
            {
                Body = new EndpointsBody
                {
                    Endpoints = _endpointsStorage
                        .LocalEndpoints
                        .Select(_ => _.EndpointData)
                        .Where(_ => _.EndpointType == EndpointType.Initializer && _.Route.NotEquals(currentRoute))
                        .ToList()
                }
            };
        }
    }
}
