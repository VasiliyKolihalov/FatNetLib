using LiteNetLib;

namespace Kolyhalov.FatNetLib.Endpoints;

public class Endpoint
{
    public Path Path { get; }
    public EndpointType EndpointType { get; }
    public DeliveryMethod DeliveryMethod { get; }

    public Endpoint(Path path, EndpointType endpointType, DeliveryMethod deliveryMethod)
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