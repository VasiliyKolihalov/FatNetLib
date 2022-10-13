using Kolyhalov.FatNetLib.Attributes;
using Kolyhalov.FatNetLib.Endpoints;
using Newtonsoft.Json;

namespace Kolyhalov.FatNetLib.Initializers.Controllers.Client;

[Route("fat-net-lib/endpoints")]
[Initial]
public class ExchangeEndpointsController : IController
{
    private readonly IEndpointsStorage _endpointsStorage;
    private readonly JsonSerializer _jsonSerializer;
    
    public ExchangeEndpointsController(IEndpointsStorage endpointsStorage, JsonSerializer jsonSerializer)
    {
        _endpointsStorage = endpointsStorage;
        _jsonSerializer = jsonSerializer;
    }
    
    [Route("exchange")]
    [Exchanger]
    public Package Exchange(Package package)
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
                ["Endpoints"] = _endpointsStorage
                    .LocalEndpoints
                    .Select(_ => _.EndpointData)
                    .Where(x => x.IsInitial == false)
            }
        };
    }
}