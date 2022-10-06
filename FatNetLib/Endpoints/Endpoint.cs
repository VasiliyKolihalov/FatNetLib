using Kolyhalov.FatNetLib.Microtypes;
using LiteNetLib;

namespace Kolyhalov.FatNetLib.Endpoints;

public class Endpoint
{
    public Route Route { get; }
    public EndpointType EndpointType { get; }
    public DeliveryMethod DeliveryMethod { get; }

    public Endpoint(Route route, EndpointType endpointType, DeliveryMethod deliveryMethod)
    {
        Route = route;
        EndpointType = endpointType;
        DeliveryMethod = deliveryMethod;
    }
}

public enum EndpointType
{
    Receiver,
    Exchanger
}