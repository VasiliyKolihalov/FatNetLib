namespace Kolyhalov.UdpFramework.Endpoints;

public interface IEndpointsStorage
{
    public List<LocalEndpoint> LocalEndpoints { get; }
    public Dictionary<int, List<Endpoint>> RemoteEndpoints { get; }
}