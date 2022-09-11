using LiteNetLib;

namespace Kolyhalov.FatNetLib.Endpoints;

public class Endpoint
{
    public string Path { get; }
    public EndpointType EndpointType { get; }
    public DeliveryMethod DeliveryMethod { get; }

    public Endpoint(string path, EndpointType endpointType, DeliveryMethod deliveryMethod)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path is null or blank");
        
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