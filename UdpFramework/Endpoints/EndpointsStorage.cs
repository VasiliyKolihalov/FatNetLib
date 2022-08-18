namespace Kolyhalov.UdpFramework.Endpoints;

public class EndpointsStorage : IEndpointsStorage
{
    private readonly List<LocalEndpoint> _localEndpoints = new();
    private readonly Dictionary<int, List<Endpoint>> _remoteEndpoints = new();


    public IEnumerable<Endpoint> GetLocalEndpointsData()
    {
        return _localEndpoints.Select(localEndpoint => localEndpoint.EndpointData).ToList();
    }

    public LocalEndpoint? GetLocalEndpointByPath(string path)
    {
        return _localEndpoints.FirstOrDefault(localEndpoint => localEndpoint.EndpointData.Path == path);
    }

    public void AddLocalEndpoint(LocalEndpoint localEndpoint)
    {
        _localEndpoints.Add(localEndpoint);
    }

    public List<Endpoint> GetRemoteEndpoints(int peerId)
    {
        return _remoteEndpoints[peerId].ToList();
    }

    public void AddRemoteEndpoints(int peerId, List<Endpoint> endpoints)
    {
        _remoteEndpoints[peerId] = endpoints.ToList();
    }

    public void RemoveRemoteEndpoints(int peerId)
    {
        _remoteEndpoints.Remove(peerId);
    }
}