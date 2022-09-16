namespace Kolyhalov.FatNetLib.Endpoints;

public class EndpointsStorage : IEndpointsStorage
{
    public IList<LocalEndpoint> LocalEndpoints { get; } = new List<LocalEndpoint>();
    public IDictionary<int, IList<Endpoint>> RemoteEndpoints { get; } = new Dictionary<int, IList<Endpoint>>();
}