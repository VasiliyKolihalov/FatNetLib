using System.Reflection;

namespace Kolyhalov.UdpFramework;

public class LocalEndpoint
{
    public Endpoint EndpointData { get; }
    public IController? Controller { get; }
    public MethodInfo Method { get;  }

    public LocalEndpoint(Endpoint endpoint, IController? controller, MethodInfo method)
    {
        EndpointData = endpoint;
        Method = method;
        Controller = controller;
    }
}