using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.LiteNetLibWrappers;
using Kolyhalov.FatNetLib.Middlewares;
using Kolyhalov.FatNetLib.ResponsePackageMonitors;

namespace Kolyhalov.FatNetLib;

public class Client : IClient
{
    private readonly IList<INetPeer> _connectedPeers;
    private readonly IEndpointsStorage _endpointsStorage;
    private readonly IResponsePackageMonitor _responsePackageMonitor;
    private readonly IMiddlewaresRunner _sendingMiddlewaresRunner;

    public Client(IList<INetPeer> connectedPeers,
        IEndpointsStorage endpointsStorage,
        IResponsePackageMonitor responsePackageMonitor,
        IMiddlewaresRunner sendingMiddlewaresRunner)
    {
        _connectedPeers = connectedPeers;
        _endpointsStorage = endpointsStorage;
        _responsePackageMonitor = responsePackageMonitor;
        _sendingMiddlewaresRunner = sendingMiddlewaresRunner;
    }

    public Package? SendPackage(Package package, int receivingPeerId)
    {
        if (package == null) throw new ArgumentNullException(nameof(package));

        INetPeer peer = _connectedPeers.FirstOrDefault(peer => peer.Id == receivingPeerId) ??
                        throw new FatNetLibException("Receiving peer not found");

        Endpoint endpoint = _endpointsStorage.RemoteEndpoints[receivingPeerId]
                                .FirstOrDefault(endpoint => endpoint.Route.Equals(package.Route)) ??
                            throw new FatNetLibException("Endpoint not found");

        if (endpoint.EndpointType == EndpointType.Exchanger && package.ExchangeId == Guid.Empty)
        {
            package.ExchangeId = Guid.NewGuid();
        }

        _sendingMiddlewaresRunner.Process(package);
        if (package.Serialized == null)
            throw new FatNetLibException($"{nameof(package.Serialized)} field is missing");
        peer.Send(package.Serialized!, endpoint.DeliveryMethod);

        return endpoint.EndpointType switch
        {
            EndpointType.Receiver => null,
            EndpointType.Exchanger => _responsePackageMonitor.Wait(package.ExchangeId),
            _ => throw new FatNetLibException($"Unsupported {nameof(EndpointType)} {endpoint.EndpointType}")
        };
    }
}