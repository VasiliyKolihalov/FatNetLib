using System.Collections.Generic;
using System.Linq;
using Kolyhalov.FatNetLib.Core.Attributes;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Wrappers;

namespace Kolyhalov.FatNetLib.Core.Controllers.Server
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
        [Schema(key: nameof(Package.Body), type: typeof(EndpointsBody))]
        public Package ExchangeEndpoints(Package handshakePackage)
        {
            INetPeer clientPeer = handshakePackage.FromPeer!;

            Package requestPackage = PackLocalEndpoints();
            requestPackage.ToPeer = clientPeer;
            Package responsePackage = handshakePackage.Courier!.Send(requestPackage)!;
            SaveClientEndpoints(responsePackage, clientPeer);

            return new Package();
        }

        private Package PackLocalEndpoints()
        {
            return new Package
            {
                Route = new Route("fat-net-lib/endpoints/exchange"),
                Body = new EndpointsBody
                {
                    Endpoints = _endpointsStorage
                        .LocalEndpoints
                        .Select(_ => _.Details)
                        .Where(_ => _.Type is EndpointType.Receiver || _.Type is EndpointType.Exchanger)
                        .ToList()
                }
            };
        }

        private void SaveClientEndpoints(Package responsePackage, INetPeer clientPeer)
        {
            IList<Endpoint> endpoints = responsePackage.GetBodyAs<EndpointsBody>().Endpoints;
            IDictionary<int, IList<Endpoint>> remoteEndpoints = _endpointsStorage.RemoteEndpoints;
            _endpointsStorage.RemoteEndpoints[clientPeer.Id] = remoteEndpoints.ContainsKey(clientPeer.Id)
                ? remoteEndpoints[clientPeer.Id].Concat(endpoints).ToList()
                : endpoints;
        }
    }
}
