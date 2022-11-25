using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Microtypes;

namespace Kolyhalov.FatNetLib.Core.Models
{
    public class Endpoint
    {
        public Route Route { get; }

        public EndpointType EndpointType { get; }

        public Reliability Reliability { get; }

        // Todo: store full schema, not only patch
        public PackageSchema RequestSchemaPatch { get; }

        public PackageSchema ResponseSchemaPatch { get; }

        public Endpoint(
            Route route,
            EndpointType endpointType,
            Reliability reliability,
            PackageSchema requestSchemaPatch,
            PackageSchema responseSchemaPatch)
        {
            Route = route;
            EndpointType = endpointType;
            Reliability = reliability;
            RequestSchemaPatch = requestSchemaPatch;
            ResponseSchemaPatch = responseSchemaPatch;
        }
    }

    public enum EndpointType
    {
        Receiver,
        Exchanger,
        Initializer,
        Event
    }
}
