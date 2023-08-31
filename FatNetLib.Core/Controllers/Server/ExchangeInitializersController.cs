using System;
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
        public EndpointsBody ExchangeInitializers([Body] EndpointsBody body, [Sender] INetPeer clientPeer)
        {
            SaveClientInitializers(body.Endpoints, clientPeer.Id);
            return PackLocalInitializers();
        }

        private void SaveClientInitializers(IList<Endpoint> endpoints, Guid clientPeerId)
        {
            IDictionary<Guid, IList<Endpoint>> remoteEndpoints = _endpointsStorage.RemoteEndpoints;
            _endpointsStorage.RemoteEndpoints[clientPeerId] = remoteEndpoints.ContainsKey(clientPeerId)
                ? remoteEndpoints[clientPeerId].Concat(endpoints).ToList()
                : endpoints;
        }

        private EndpointsBody PackLocalInitializers()
        {
            var currentRoute = new Route("fat-net-lib/initializers/exchange");
            return new EndpointsBody
            {
                Endpoints = _endpointsStorage
                    .LocalEndpoints
                    .Select(_ => _.Details)
                    .Where(_ => _.Type == EndpointType.Initializer && _.Route.NotEquals(currentRoute))
                    .ToList()
            };
        }
    }
}
