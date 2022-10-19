using Kolyhalov.FatNetLib.Attributes;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Microtypes;

namespace Kolyhalov.FatNetLib.Initializers.Controllers.Server;

[Route("fat-net-lib/endpoints")]
[Initial]
public class ExchangeEndpointsController : IController
{
    private readonly IEndpointsStorage _endpointsStorage;
    private readonly IClient _client;

    public ExchangeEndpointsController(IEndpointsStorage endpointsStorage, IClient client)
    {
        _endpointsStorage = endpointsStorage;
        _client = client;
    }

    [Route("exchange")]
    [Exchanger]
    [Schema(key: nameof(Package.Body), type: typeof(EndpointsBody))]
    [return: Schema(key: nameof(Package.Body), type: typeof(EndpointsBody))]
    public Package ExchangeEndpoints(Package handshakePackage)
    {
        int clientPeerId = handshakePackage.FromPeerId!.Value;

        Package requestPackage = PackLocalEndpoints();
        requestPackage.ToPeerId = clientPeerId;
        Package responsePackage = _client.SendPackage(requestPackage)!;
        SaveClientEndpoints(responsePackage, clientPeerId);

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
                    .Select(_ => _.EndpointData)
                    .Where(_ => _.IsInitial == false)
                    .ToList()
            }
        };
    }

    private void SaveClientEndpoints(Package responsePackage, int clientPeerId)
    {
        IList<Endpoint> endpoints = responsePackage.GetBodyAs<EndpointsBody>()!.Endpoints;        IDictionary<int, IList<Endpoint>> remoteEndpoints = _endpointsStorage.RemoteEndpoints;
        _endpointsStorage.RemoteEndpoints[clientPeerId] = remoteEndpoints.ContainsKey(clientPeerId)
            ? remoteEndpoints[clientPeerId].Concat(endpoints).ToList()
            : endpoints;
    }
}