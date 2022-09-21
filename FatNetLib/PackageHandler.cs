using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Middlewares;
using Kolyhalov.FatNetLib.NetPeers;
using LiteNetLib;
using LiteNetLib.Utils;
using Newtonsoft.Json;

namespace Kolyhalov.FatNetLib;

public class PackageHandler : IPackageHandler
{
    private readonly IEndpointsStorage _endpointsStorage;
    private readonly IEndpointsInvoker _endpointsInvoker;
    private readonly IMiddlewaresRunner _receivingMiddlewaresRunner;
    private readonly IMiddlewaresRunner _sendingMiddlewaresRunner;
    private readonly IList<INetPeer> _connectedPeers;

    public PackageHandler(IEndpointsStorage endpointsStorage,
        IEndpointsInvoker endpointsInvoker,
        IMiddlewaresRunner receivingMiddlewaresRunner,
        IMiddlewaresRunner sendingMiddlewaresRunner,
        IList<INetPeer> connectedPeers)
    {
        _endpointsStorage = endpointsStorage;
        _endpointsInvoker = endpointsInvoker;
        _receivingMiddlewaresRunner = receivingMiddlewaresRunner;
        _sendingMiddlewaresRunner = sendingMiddlewaresRunner;
        _connectedPeers = connectedPeers;
    }

    public void Handle(Package requestPackage, int peerId, DeliveryMethod deliveryMethod)
    {
        LocalEndpoint? endpoint = _endpointsStorage.LocalEndpoints
            .FirstOrDefault(_ => _.EndpointData.Path == requestPackage.Route!);
        if (endpoint == null)
        {
            throw new FatNetLibException(
                $"Package from {peerId} pointed to a non-existent endpoint. Route: {requestPackage.Route}");
        }

        if (endpoint.EndpointData.DeliveryMethod != deliveryMethod)
        {
            throw new FatNetLibException($"Package from {peerId} came with the wrong type of delivery");
        }

        requestPackage = _receivingMiddlewaresRunner.Process(requestPackage);
        if (endpoint.EndpointData.EndpointType != EndpointType.Exchanger)
        {
            _endpointsInvoker.InvokeReceiver(endpoint, requestPackage);
            return;
        }

        Package responsePackage = _endpointsInvoker.InvokeExchanger(endpoint, requestPackage);

        responsePackage.Route = requestPackage.Route;
        responsePackage.ExchangeId = requestPackage.ExchangeId;
        responsePackage.IsResponse = true;

        responsePackage = _sendingMiddlewaresRunner.Process(responsePackage);

        //todo: serialization and deserialization middleware
        INetPeer peer = _connectedPeers.Single(netPeer => netPeer.Id == peerId);
        string jsonPackage = JsonConvert.SerializeObject(responsePackage);
        var writer = new NetDataWriter();
        writer.Put(jsonPackage);
        peer.Send(writer, deliveryMethod);
    }
}