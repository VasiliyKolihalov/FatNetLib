using LiteNetLib;

namespace Kolyhalov.UdpFramework;

public class Endpoint
{
    public string Path { get; }
    public EndpointType EndpointType { get; }
    public DeliveryMethod DeliveryMethod { get; }

    public Endpoint(string path, EndpointType endpointType, DeliveryMethod deliveryMethod)
    {
        Path = path;
        EndpointType = endpointType;
        DeliveryMethod = deliveryMethod;
    }
}

public enum EndpointType
{
    Receiver,
    Exchanger
}