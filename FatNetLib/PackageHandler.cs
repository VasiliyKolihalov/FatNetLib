using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.LiteNetLibWrappers;
using Kolyhalov.FatNetLib.Middlewares;
using LiteNetLib;

namespace Kolyhalov.FatNetLib;

public class PackageHandler : IPackageHandler
{
    private readonly IEndpointsStorage _endpointsStorage;
    private readonly IEndpointsInvoker _endpointsInvoker;
    private readonly IMiddlewaresRunner _sendingMiddlewaresRunner;
    private readonly IList<INetPeer> _connectedPeers;

    public PackageHandler(IEndpointsStorage endpointsStorage,
        IEndpointsInvoker endpointsInvoker,
        IMiddlewaresRunner sendingMiddlewaresRunner,
        IList<INetPeer> connectedPeers)
    {
        _endpointsStorage = endpointsStorage;
        _endpointsInvoker = endpointsInvoker;
        _sendingMiddlewaresRunner = sendingMiddlewaresRunner;
        _connectedPeers = connectedPeers;
    }

    public void Handle(Package requestPackage, int peerId, DeliveryMethod deliveryMethod)
    {
        LocalEndpoint? endpoint =
            _endpointsStorage.LocalEndpoints.FirstOrDefault(_ => _.EndpointData.Route.Equals(requestPackage.Route));
        if (endpoint == null)
        {
            throw new FatNetLibException(
                $"Package from {peerId} pointed to a non-existent endpoint. Route: {requestPackage.Route}");
        }

        if (endpoint.EndpointData.DeliveryMethod != deliveryMethod)
        {
            throw new FatNetLibException($"Package from {peerId} came with the wrong type of delivery");
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

        _sendingMiddlewaresRunner.Process(responsePackage);

        _connectedPeers.Single(netPeer => netPeer.Id == peerId)
            .Send(responsePackage.Serialized!, deliveryMethod);
    }
}