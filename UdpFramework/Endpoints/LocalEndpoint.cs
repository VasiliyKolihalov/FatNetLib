using System.Reflection;

namespace Kolyhalov.UdpFramework.Endpoints;

public class LocalEndpoint
{
    public Endpoint EndpointData { get; }
    public Delegate MethodDelegate { get; }
    public ParameterInfo[] Parameters { get; }
    
    public LocalEndpoint(Endpoint endpoint, Delegate methodDelegate)
    {
        EndpointData = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        MethodDelegate = methodDelegate ?? throw new ArgumentNullException(nameof(methodDelegate));
        Parameters = MethodDelegate.Method.GetParameters();
    }
}