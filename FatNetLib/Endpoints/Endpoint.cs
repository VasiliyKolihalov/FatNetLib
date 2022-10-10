using Kolyhalov.FatNetLib.Microtypes;
using LiteNetLib;

namespace Kolyhalov.FatNetLib.Endpoints;

public class Endpoint
{
    public Route Route { get; }
    public EndpointType EndpointType { get; }
    public DeliveryMethod DeliveryMethod { get; }
    public bool IsInitial { get; }

    public Endpoint(Route route, EndpointType endpointType, DeliveryMethod deliveryMethod, bool isInitial)
    {
        Route = route;
        EndpointType = endpointType;
        DeliveryMethod = deliveryMethod;
        IsInitial = isInitial;
    }
}

public enum EndpointType
{
    Receiver,
    Exchanger
}