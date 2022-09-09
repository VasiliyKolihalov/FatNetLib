namespace Kolyhalov.FatNetLib.Endpoints;

public interface IEndpointsStorage
{
    public List<LocalEndpoint> LocalEndpoints { get; }
    public Dictionary<int, List<Endpoint>> RemoteEndpoints { get; }
}