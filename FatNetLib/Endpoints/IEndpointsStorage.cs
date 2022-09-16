namespace Kolyhalov.FatNetLib.Endpoints;

public interface IEndpointsStorage
{
    public IList<LocalEndpoint> LocalEndpoints { get; }
    public IDictionary<int, IList<Endpoint>> RemoteEndpoints { get; }
}