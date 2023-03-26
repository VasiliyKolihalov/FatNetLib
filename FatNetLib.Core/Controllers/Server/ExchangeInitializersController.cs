using System.Collections.Generic;
using System.Linq;
using Kolyhalov.FatNetLib.Core.Attributes;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Utils;
using Kolyhalov.FatNetLib.Core.Wrappers;

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
        public Package ExchangeInitializers([Body] EndpointsBody body, [Sender] INetPeer clientPeer)
        {
            SaveClientInitializers(body.Endpoints, clientPeer.Id);
            return PackLocalInitializers();
        }

        private void SaveClientInitializers(IList<Endpoint> endpoints, int clientPeerId)
        {
            IDictionary<int, IList<Endpoint>> remoteEndpoints = _endpointsStorage.RemoteEndpoints;
            _endpointsStorage.RemoteEndpoints[clientPeerId] = remoteEndpoints.ContainsKey(clientPeerId)
                ? remoteEndpoints[clientPeerId].Concat(endpoints).ToList()
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
                        .Select(_ => _.Details)
                        .Where(_ => _.Type == EndpointType.Initializer && _.Route.NotEquals(currentRoute))
                        .ToList()
                }
            };
        }
    }
}
