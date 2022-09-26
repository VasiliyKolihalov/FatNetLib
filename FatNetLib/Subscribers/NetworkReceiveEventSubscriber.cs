using Kolyhalov.FatNetLib.LiteNetLibWrappers;
using Kolyhalov.FatNetLib.Middlewares;
using Kolyhalov.FatNetLib.ResponsePackageMonitors;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Kolyhalov.FatNetLib.Subscribers;

public class NetworkReceiveEventSubscriber : INetworkReceiveEventSubscriber
{
    private readonly IPackageHandler _packageHandler;
    private readonly IResponsePackageMonitor _responsePackageMonitor;
    private readonly IMiddlewaresRunner _receivingMiddlewaresRunner;
    private readonly PackageSchema _defaultDefaultPackageSchema;

    public NetworkReceiveEventSubscriber(IPackageHandler packageHandler,
        IResponsePackageMonitor responsePackageMonitor,
        IMiddlewaresRunner receivingMiddlewaresRunner, 
        PackageSchema defaultPackageSchema)
    {
        _packageHandler = packageHandler;
        _responsePackageMonitor = responsePackageMonitor;
        _receivingMiddlewaresRunner = receivingMiddlewaresRunner;
        _defaultDefaultPackageSchema = defaultPackageSchema;
    }

    public void Handle(INetPeer peer, NetDataReader reader, DeliveryMethod deliveryMethod)
    {
        var package = new Package
        {
            Serialized = reader.GetString(),
            Schema = _defaultDefaultPackageSchema
        };
        _receivingMiddlewaresRunner.Process(package);

        if (package.Route!.Contains("connection"))
            return;

        if (package.IsResponse)
        {
            _responsePackageMonitor.Pulse(package);
        }
        else
        {
            _packageHandler.Handle(package, peer.Id, deliveryMethod);
        }
    }
}