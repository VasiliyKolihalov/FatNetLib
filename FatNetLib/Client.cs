using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Middlewares;
using Kolyhalov.FatNetLib.NetPeers;
using Kolyhalov.FatNetLib.ResponsePackageMonitors;
using LiteNetLib.Utils;
using Newtonsoft.Json;

namespace Kolyhalov.FatNetLib;

public class Client : IClient
{
    private readonly IList<INetPeer> _connectedPeers;
    private readonly IEndpointsStorage _endpointsStorage;
    private readonly IResponsePackageMonitor _responsePackageMonitor;
    private readonly IMiddlewaresRunner _sendingMiddlewaresRunner;
    private readonly IMiddlewaresRunner _receivingMiddlewaresRunner;

    public Client(IList<INetPeer> connectedPeers,
        IEndpointsStorage endpointsStorage,
        IResponsePackageMonitor responsePackageMonitor,
        IMiddlewaresRunner sendingMiddlewaresRunner,
        IMiddlewaresRunner receivingMiddlewaresRunner)
    {
        _connectedPeers = connectedPeers;
        _endpointsStorage = endpointsStorage;
        _responsePackageMonitor = responsePackageMonitor;
        _sendingMiddlewaresRunner = sendingMiddlewaresRunner;
        _receivingMiddlewaresRunner = receivingMiddlewaresRunner;
    }

    public Package? SendPackage(Package package, int receivingPeerId)
    {
        if (package == null) throw new ArgumentNullException(nameof(package));

        INetPeer peer = _connectedPeers.FirstOrDefault(peer => peer.Id == receivingPeerId) ??
                        throw new FatNetLibException("Receiving peer not found");

        Endpoint endpoint = _endpointsStorage.RemoteEndpoints[receivingPeerId]
                                .FirstOrDefault(endpoint => endpoint.Path == package.Route) ??
                            throw new FatNetLibException("Endpoint not found");

        if (endpoint.EndpointType == EndpointType.Exchanger && package.ExchangeId == null)
        {
            package.ExchangeId = Guid.NewGuid();
        }

        package = _sendingMiddlewaresRunner.Process(package);

        // Todo: ticket #52 serialization and deserialization middleware
        string jsonPackage = JsonConvert.SerializeObject(package);
        var writer = new NetDataWriter();
        writer.Put(jsonPackage);
        peer.Send(writer, endpoint.DeliveryMethod);

        if (endpoint.EndpointType == EndpointType.Receiver)
            return null;

        Guid exchangeId = package.ExchangeId!.Value;
        Package responsePackage = _responsePackageMonitor.Wait(exchangeId);
        responsePackage = _receivingMiddlewaresRunner.Process(responsePackage);
        return responsePackage;
    }
}