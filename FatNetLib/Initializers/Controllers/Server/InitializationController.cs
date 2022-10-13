using Kolyhalov.FatNetLib.Attributes;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Microtypes;
using Newtonsoft.Json;

namespace Kolyhalov.FatNetLib.Initializers.Controllers.Server;

[Route("fat-net-lib")]
[Initial]
public class InitializationController : IController
{
    private readonly IEndpointsStorage _endpointsStorage;
    private readonly JsonSerializer _jsonSerializer;

    public InitializationController(IEndpointsStorage endpointsStorage, JsonSerializer jsonSerializer)
    {
        _endpointsStorage = endpointsStorage;
        _jsonSerializer = jsonSerializer;
    }

    [Route("init-endpoints/exchange")]
    [Exchanger]
    public Package ExchangeInitEndpoints(Package package)
    {
        int fromPeerId = package.FromPeerId!.Value;
        var endpointsJson = package.Body!["Endpoints"].ToString()!;
        JsonConverter[] jsonConverters = _jsonSerializer.Converters.ToArray();
        var endpoints = JsonConvert.DeserializeObject<IList<Endpoint>>(endpointsJson, jsonConverters)!;
        IDictionary<int, IList<Endpoint>> remoteEndpoints = _endpointsStorage.RemoteEndpoints;
        _endpointsStorage.RemoteEndpoints[fromPeerId] = remoteEndpoints.ContainsKey(fromPeerId)
            ? remoteEndpoints[fromPeerId].Concat(endpoints).ToList()
            : endpoints;

        var currentRoute = new Route("fat-net-lib/init-endpoints/exchange");
        return new Package
        {
            Body = new Dictionary<string, object>
            {
                {
                    "Endpoints", _endpointsStorage.LocalEndpoints.Select(x => x.EndpointData)
                        .Where(x => x.IsInitial && !x.Route.Equals(currentRoute))
                        .Select(x => x.Route)
                }
            }
        };
    }
}