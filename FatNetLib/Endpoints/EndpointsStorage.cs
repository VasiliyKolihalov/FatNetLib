namespace Kolyhalov.FatNetLib.Endpoints;

public class EndpointsStorage : IEndpointsStorage
{
    public List<LocalEndpoint> LocalEndpoints { get; } = new();
    public Dictionary<int, List<Endpoint>> RemoteEndpoints { get; } = new();
}