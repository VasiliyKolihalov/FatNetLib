using System.Reflection;

namespace Kolyhalov.FatNetLib.Endpoints;

public class LocalEndpoint
{
    public Endpoint EndpointData { get; }

    public Delegate MethodDelegate { get; }

    public ParameterInfo[] Parameters { get; }

    public LocalEndpoint(Endpoint endpoint, Delegate methodDelegate)
    {
        EndpointData = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        MethodDelegate = methodDelegate ?? throw new ArgumentNullException(nameof(methodDelegate));
        if (EndpointData.EndpointType is EndpointType.Exchanger && methodDelegate.Method.ReturnType != typeof(Package))
            throw new FatNetLibException("Return type of exchanger should be Package");
        Parameters = MethodDelegate.Method.GetParameters();
    }
}
