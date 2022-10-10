using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Middlewares;
using Kolyhalov.FatNetLib.Wrappers;

namespace Kolyhalov.FatNetLib;

public class PackageHandler : IPackageHandler
{
    private readonly IEndpointsStorage _endpointsStorage;
    private readonly IEndpointsInvoker _endpointsInvoker;
    private readonly IMiddlewaresRunner _sendingMiddlewaresRunner;
    private readonly IList<INetPeer> _connectedPeers;
    private readonly DependencyContext _context;

    public PackageHandler(IEndpointsStorage endpointsStorage,
        IEndpointsInvoker endpointsInvoker,
        IMiddlewaresRunner sendingMiddlewaresRunner,
        IList<INetPeer> connectedPeers, 
        DependencyContext context)
    {
        _endpointsStorage = endpointsStorage;
        _endpointsInvoker = endpointsInvoker;
        _sendingMiddlewaresRunner = sendingMiddlewaresRunner;
        _connectedPeers = connectedPeers;
        _context = context;
    }

    public void Handle(Package requestPackage)
    {
        LocalEndpoint? endpoint =
            _endpointsStorage.LocalEndpoints.FirstOrDefault(_ => _.EndpointData.Route.Equals(requestPackage.Route));
        if (endpoint == null)
        {
            throw new FatNetLibException(
                $"Package from {requestPackage.FromPeerId} pointed to a non-existent endpoint. Route: {requestPackage.Route}");
        }

        if (endpoint.EndpointData.DeliveryMethod != requestPackage.DeliveryMethod)
        {
            throw new FatNetLibException($"Package from {requestPackage.FromPeerId} came with the wrong type of delivery");
        }

        if (endpoint.EndpointData.EndpointType == EndpointType.Receiver)
        {
            _endpointsInvoker.InvokeReceiver(endpoint, requestPackage);
            return;
        }

        Package responsePackage = _endpointsInvoker.InvokeExchanger(endpoint, requestPackage);

        responsePackage.Route = requestPackage.Route;
        responsePackage.ExchangeId = requestPackage.ExchangeId;
        responsePackage.IsResponse = true;
        responsePackage.Context = _context;
        responsePackage.ToPeerId = requestPackage.FromPeerId;
        responsePackage.DeliveryMethod = requestPackage.DeliveryMethod;

        _sendingMiddlewaresRunner.Process(responsePackage);

        _connectedPeers.Single(netPeer => netPeer.Id == responsePackage.ToPeerId)
            .Send(responsePackage);
    }
}