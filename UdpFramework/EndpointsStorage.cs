using System.Reflection;

namespace Kolyhalov.UdpFramework;

public class EndpointsStorage : IEndpointsStorage
{
    private readonly List<LocalEndpoint> _localEndpoints = new();
    private readonly Dictionary<int, List<Endpoint>> _remoteEndpoints = new();


    public IEnumerable<Endpoint> GetLocalEndpointsData()
    {
        return _localEndpoints.Select(localEndpoint => localEndpoint.EndpointData).ToList();
    }

    public LocalEndpoint? GetLocalEndpointFromPath(string path)
    {
        LocalEndpoint? localEndpoint = _localEndpoints.FirstOrDefault(localEndpoint => localEndpoint.EndpointData.Path == path);
        return localEndpoint;
    }

    public void AddLocalEndpoint(LocalEndpoint localEndpoint)
    {
        _localEndpoints.Add(localEndpoint);
    }

    public List<Endpoint> GetRemoteEndpoints(int peerId)
    {
        List<Endpoint> endpoints = _remoteEndpoints[peerId];
        return endpoints.ToList();
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