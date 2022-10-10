using Kolyhalov.FatNetLib.Attributes;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Microtypes;
using Newtonsoft.Json;

namespace Kolyhalov.FatNetLib.InitialControllers.Server;

[Route("fat-net-lib")]
public class InitializationController : IController
{
    private readonly IEndpointsStorage _endpointsStorage;
    private readonly JsonSerializer _jsonSerializer;

    public InitializationController(IEndpointsStorage endpointsStorage, JsonSerializer jsonSerializer)
    {
        _endpointsStorage = endpointsStorage;
        _jsonSerializer = jsonSerializer;
    }

    [Route("init-endpoints/hold-and-get")]
    [Exchanger]
    public Package GetInitEndpoints(Package package)
    {
        int fromPeerId = package.FromPeerId!.Value;
        var endpointsJson = package.Body!["Endpoints"].ToString()!;
        JsonConverter[] jsonConverters = _jsonSerializer.Converters.ToArray();
        var endpoints = JsonConvert.DeserializeObject<IList<Endpoint>>(endpointsJson, jsonConverters)!;
        IDictionary<int, IList<Endpoint>> remoteEndpoints = _endpointsStorage.RemoteEndpoints;
        _endpointsStorage.RemoteEndpoints[fromPeerId] = remoteEndpoints.ContainsKey(fromPeerId)
            ? remoteEndpoints[fromPeerId].Concat(endpoints).ToList()
            : endpoints;

        return new Package
        {
            Body = new Dictionary<string, object>
            {
                {
                    "Endpoints", _endpointsStorage.LocalEndpoints
                        .Where(x => x.EndpointData.IsInitial && 
                                    !x.EndpointData.Route.Equals(new Route("fat-net-lib/init-endpoints/hold-and-get")))
                        .Select(x => x.EndpointData.Route)
                }
            }
        };
    }
}