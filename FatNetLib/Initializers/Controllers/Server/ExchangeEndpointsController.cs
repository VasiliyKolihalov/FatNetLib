using Kolyhalov.FatNetLib.Attributes;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Microtypes;
using Newtonsoft.Json;

namespace Kolyhalov.FatNetLib.Initializers.Controllers.Server;

[Route("fat-net-lib/endpoints")]
[Initial]
public class ExchangeEndpointsController : IController
{
    private readonly IEndpointsStorage _endpointsStorage;
    private readonly IClient _client;
    private readonly JsonSerializer _jsonSerializer;

    public ExchangeEndpointsController(IEndpointsStorage endpointsStorage, IClient client,
        JsonSerializer jsonSerializer)
    {
        _endpointsStorage = endpointsStorage;
        _client = client;
        _jsonSerializer = jsonSerializer;
    }

    [Route("exchange")]
    [Exchanger]
    public Package Exchange(Package package)
    {
        int fromPeerId = package.FromPeerId!.Value;
        var requestPackage = new Package
        {
            Route = new Route("fat-net-lib/endpoints/exchange"),
            Body = new Dictionary<string, object>
            {
                ["Endpoints"] = _endpointsStorage
                    .LocalEndpoints
                    .Select(_ => _.EndpointData)
                    .Where(x => x.IsInitial == false)
            },
            ToPeerId = fromPeerId
        };
        Package clientResponsePackage = _client.SendPackage(requestPackage)!;

        var endpointsJson = clientResponsePackage.Body!["Endpoints"].ToString()!;
        JsonConverter[] jsonConverters = _jsonSerializer.Converters.ToArray();
        var endpoints = JsonConvert.DeserializeObject<IList<Endpoint>>(endpointsJson, jsonConverters)!;
        IDictionary<int, IList<Endpoint>> remoteEndpoints = _endpointsStorage.RemoteEndpoints;
        _endpointsStorage.RemoteEndpoints[fromPeerId] = remoteEndpoints.ContainsKey(fromPeerId)
            ? remoteEndpoints[fromPeerId].Concat(endpoints).ToList()
            : endpoints;

        return new Package();
    }
}