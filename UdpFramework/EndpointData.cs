using LiteNetLib;

namespace UdpFramework;

public class EndpointData
{
    public string Path { get; }
    public EndpointType EndpointType { get; }
    public DeliveryMethod DeliveryMethod { get; }

    public EndpointData(string path, EndpointType endpointType, DeliveryMethod deliveryMethod)
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