using Kolyhalov.FatNetLib.Core.Microtypes;

namespace Kolyhalov.FatNetLib.Core.Endpoints
{
    public class Endpoint
    {
        public Route Route { get; }

        public EndpointType EndpointType { get; }

        public Reliability Reliability { get; }

        public bool IsInitial { get; }

        // Todo: store full schema, not only patch
        public PackageSchema RequestSchemaPatch { get; }

        public PackageSchema ResponseSchemaPatch { get; }

        public Endpoint(
            Route route,
            EndpointType endpointType,
            Reliability reliability,
            bool isInitial,
            PackageSchema requestSchemaPatch,
            PackageSchema responseSchemaPatch)
        {
            Route = route;
            EndpointType = endpointType;
            Reliability = reliability;
            IsInitial = isInitial;
            RequestSchemaPatch = requestSchemaPatch;
            ResponseSchemaPatch = responseSchemaPatch;
        }
    }

    public enum EndpointType
    {
        Receiver,
        Exchanger
    }
}
