using Kolyhalov.FatNetLib.Attributes;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Microtypes;

namespace Kolyhalov.FatNetLib.Initializers.Controllers.Server;

[Route("fat-net-lib")]
[Initial]
public class ExchangeInitialEndpointsController : IController
{
    private readonly IEndpointsStorage _endpointsStorage;

    public ExchangeInitialEndpointsController(IEndpointsStorage endpointsStorage)
    {
        _endpointsStorage = endpointsStorage;
    }

    [Route("init-endpoints/exchange")]
    [Schema(key: nameof(Package.Body), type: typeof(EndpointsBody))]
    [return: Schema(key: nameof(Package.Body), type: typeof(EndpointsBody))]
    public Package ExchangeInitialEndpoints(Package package)
    {
        SaveClientEndpoints(package);
        return PackLocalEndpoints();
    }

    private void SaveClientEndpoints(Package package)
    {
        int fromPeerId = package.FromPeerId!.Value;
        IList<Endpoint> endpoints = package.GetBodyAs<EndpointsBody>()!.Endpoints;
        IDictionary<int, IList<Endpoint>> remoteEndpoints = _endpointsStorage.RemoteEndpoints;
        _endpointsStorage.RemoteEndpoints[fromPeerId] = remoteEndpoints.ContainsKey(fromPeerId)
            ? remoteEndpoints[fromPeerId].Concat(endpoints).ToList()
            : endpoints;
    }

    private Package PackLocalEndpoints()
    {
        var currentRoute = new Route("fat-net-lib/init-endpoints/exchange");
        return new Package
        {
            Body = new EndpointsBody
            {
                Endpoints = _endpointsStorage
                    .LocalEndpoints
                    .Select(_ => _.EndpointData)
                    .Where(_ => _.IsInitial && !_.Route.Equals(currentRoute))
                    .ToList()
            }
        };
    }
}
