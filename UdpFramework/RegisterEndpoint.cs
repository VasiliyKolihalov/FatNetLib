using System.Reflection;
using LiteNetLib;

namespace UdpFramework;

public class RegisterEndpoint
{
    public EndpointData Data { get; }
    public Controller? Controller { get; }
    public MethodInfo Method { get;  }

    public RegisterEndpoint(string path, EndpointType endpointType,DeliveryMethod deliveryMethod, Controller controller, MethodInfo method)
    {
        Data = new EndpointData(path, endpointType, deliveryMethod);
        Method = method;
        Controller = controller;
    }
}