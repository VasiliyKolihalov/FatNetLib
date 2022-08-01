
using System.Reflection;

namespace Kolyhalov.UdpFramework;

public class LocalEndpoint
{
    public Endpoint EndpointData { get; }
    public Delegate MethodDelegate { get; }
    public List<Type> ParameterTypes { get; }

    public LocalEndpoint(Endpoint endpoint, Delegate methodDelegate)
    {
        EndpointData = endpoint;
        MethodDelegate = methodDelegate;
        ParameterTypes = MethodDelegate.Method.GetParameters().Select(x => x.ParameterType).ToList();
    }
}