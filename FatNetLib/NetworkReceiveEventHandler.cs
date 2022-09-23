using Kolyhalov.FatNetLib.NetPeers;
using Kolyhalov.FatNetLib.ResponsePackageMonitors;
using LiteNetLib;
using LiteNetLib.Utils;
using Newtonsoft.Json;

namespace Kolyhalov.FatNetLib;

public class NetworkReceiveEventHandler : INetworkReceiveEventHandler
{
    private readonly IPackageHandler _packageHandler;
    private readonly IResponsePackageMonitor _responsePackageMonitor;

    public NetworkReceiveEventHandler(IPackageHandler packageHandler, IResponsePackageMonitor responsePackageMonitor)
    {
        _packageHandler = packageHandler;
        _responsePackageMonitor = responsePackageMonitor;
    }

    public void Handle(INetPeer peer, NetDataReader reader, DeliveryMethod deliveryMethod)
    {
        string jsonPackage = reader.GetString();
        Package package;
        try
        {
            package = JsonConvert.DeserializeObject<Package>(jsonPackage)
                      ?? throw new FatNetLibException("Deserialized package is null");
        }
        catch (Exception exception)
        {
            throw new FatNetLibException("Failed to deserialize package", exception);
        }

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