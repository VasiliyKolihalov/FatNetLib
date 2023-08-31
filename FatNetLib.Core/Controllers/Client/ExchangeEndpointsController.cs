using System;
using System.Collections.Generic;
using System.Linq;
using Kolyhalov.FatNetLib.Core.Attributes;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Wrappers;

namespace Kolyhalov.FatNetLib.Core.Controllers.Client
{
    [Route("fat-net-lib/endpoints")]
    public class ExchangeEndpointsController : IController
    {
        private readonly IEndpointsStorage _endpointsStorage;

        public ExchangeEndpointsController(IEndpointsStorage endpointsStorage)
        {
            _endpointsStorage = endpointsStorage;
        }

        [Initializer]
        [Route("exchange")]
        public EndpointsBody ExchangeEndpoints([Body] EndpointsBody body, [Sender] INetPeer serverPeer)
        {
            SaveServerEndpoints(body.Endpoints, serverPeer.Id);
            return PackLocalEndpoints();
        }

        private void SaveServerEndpoints(IList<Endpoint> endpoints, Guid serverPeerId)
        {
            IDictionary<Guid, IList<Endpoint>> remoteEndpoints = _endpointsStorage.RemoteEndpoints;
            _endpointsStorage.RemoteEndpoints[serverPeerId] = remoteEndpoints.ContainsKey(serverPeerId)
                ? remoteEndpoints[serverPeerId].Concat(endpoints).ToList()
                : endpoints;
        }

        private EndpointsBody PackLocalEndpoints()
        {
            return new EndpointsBody
            {
                Endpoints = _endpointsStorage
                    .LocalEndpoints
                    .Select(_ => _.Details)
                    .Where(_ => _.Type is EndpointType.Consumer || _.Type is EndpointType.Exchanger)
                    .ToList()
            };
        }
    }
}
