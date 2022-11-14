using System.Collections.Generic;
using System.Linq;
using Kolyhalov.FatNetLib.Core.Attributes;
using Kolyhalov.FatNetLib.Core.Storages;

namespace Kolyhalov.FatNetLib.Core.Controllers.Client
{
    [Route("fat-net-lib/endpoints")]
    [Initial]
    public class ExchangeEndpointsController : IController
    {
        private readonly IEndpointsStorage _endpointsStorage;

        public ExchangeEndpointsController(IEndpointsStorage endpointsStorage)
        {
            _endpointsStorage = endpointsStorage;
        }

        [Route("exchange")]
        [Schema(key: nameof(Package.Body), type: typeof(EndpointsBody))]
        [return: Schema(key: nameof(Package.Body), type: typeof(EndpointsBody))]
        public Package ExchangeEndpoints(Package package)
        {
            SaveServerEndpoints(package);
            return PackLocalEndpoints();
        }

        private void SaveServerEndpoints(Package package)
        {
            int fromPeerId = package.FromPeerId!.Value;
            IList<Endpoint> endpoints = package.GetBodyAs<EndpointsBody>().Endpoints;
            IDictionary<int, IList<Endpoint>> remoteEndpoints = _endpointsStorage.RemoteEndpoints;
            _endpointsStorage.RemoteEndpoints[fromPeerId] = remoteEndpoints.ContainsKey(fromPeerId)
                ? remoteEndpoints[fromPeerId].Concat(endpoints).ToList()
                : endpoints;
        }

        private Package PackLocalEndpoints()
        {
            return new Package
            {
                Body = new EndpointsBody
                {
                    Endpoints = _endpointsStorage
                        .LocalEndpoints
                        .Select(_ => _.EndpointData)
                        .Where(_ => _.IsInitial == false)
                        .ToList()
                }
            };
        }
    }
}
