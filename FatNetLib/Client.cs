using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Middlewares;
using Kolyhalov.FatNetLib.Monitors;
using Kolyhalov.FatNetLib.Wrappers;

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

    public Package? SendPackage(Package package)
    {
        if (package == null) throw new ArgumentNullException(nameof(package));

        int toPeerId = package.ToPeerId
                       ?? throw new ArgumentNullException(nameof(package.ToPeerId));

        INetPeer peer = _connectedPeers.FirstOrDefault(peer => peer.Id == toPeerId) ??
                        throw new FatNetLibException("Receiving peer not found");

        Endpoint endpoint = _endpointsStorage.RemoteEndpoints[toPeerId]
                                .FirstOrDefault(endpoint => endpoint.Route.Equals(package.Route)) ??
                            throw new FatNetLibException("Endpoint not found");

        package.DeliveryMethod = endpoint.DeliveryMethod;
        if (endpoint.EndpointType == EndpointType.Exchanger && package.ExchangeId == Guid.Empty)
        {
            package.ExchangeId = Guid.NewGuid();
        }

        _sendingMiddlewaresRunner.Process(package);
        if (package.Serialized == null)
            throw new FatNetLibException($"{nameof(package.Serialized)} field is missing");
        peer.Send(package);

        return endpoint.EndpointType switch
        {
            EndpointType.Receiver => null,
            EndpointType.Exchanger => _responsePackageMonitor.Wait(package.ExchangeId),
            _ => throw new FatNetLibException($"Unsupported {nameof(EndpointType)} {endpoint.EndpointType}")
        };
    }
}