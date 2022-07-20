using System.Reflection;
using LiteNetLib;

namespace UdpFramework;

public class LocalEndpoint
{
    public Endpoint Data { get; }
    public IController? Controller { get; }
    public MethodInfo Method { get;  }

    public LocalEndpoint(Endpoint endpoint, IController controller, MethodInfo method)
    {
        Data = endpoint;
        Method = method;
        Controller = controller;
    }
}