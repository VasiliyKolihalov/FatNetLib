namespace Kolyhalov.UdpFramework;

public interface IEndpointsStorage
{
    public IEnumerable<Endpoint> GetLocalEndpointsData();
    public LocalEndpoint? GetLocalEndpointByPath(string path);
    public void AddLocalEndpoint(LocalEndpoint localEndpoint);

    public List<Endpoint> GetRemoteEndpoints(int peerId);
    public void AddRemoteEndpoints(int peerId, List<Endpoint> endpoints);
    public void RemoveRemoteEndpoints(int peerId);
}