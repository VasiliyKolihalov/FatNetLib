using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.NetPeers;
using LiteNetLib;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static Kolyhalov.FatNetLib.ExceptionUtils;
using NetPeer = Kolyhalov.FatNetLib.NetPeers.NetPeer;


namespace Kolyhalov.FatNetLib;

public class ClientListener : NetEventListener
{
    public ClientListener(EventBasedNetListener listener,
        INetworkReceiveEventHandler receiverEventHandler,
        NetManager netManager,
        IList<INetPeer> connectedPeers,
        IEndpointsStorage endpointsStorage,
        ILogger? logger,
        ClientConfiguration configuration,
        PackageSchema defaultPackageSchema) : base(listener,
        receiverEventHandler,
        netManager,
        connectedPeers,
        endpointsStorage,
        logger,
        defaultPackageSchema)
    {
        Configuration = configuration;
    }

    protected override ClientConfiguration Configuration { get; }

    protected override void StartListen()
    {
        Listener.PeerConnectedEvent += peer =>
            CatchExceptionsTo(Logger, () =>
                ConnectedPeers.Add(new NetPeer(peer)));
        Listener.PeerDisconnectedEvent += (peer, _) =>
            CatchExceptionsTo(Logger, () =>
                ConnectedPeers.Remove(new NetPeer(peer)));

        RegisterConnectionEvent();

        NetManager.Start();
        NetManager.Connect(Configuration.Address,
            Configuration.Port.Value,
            Configuration.ConnectionKey);
    }

    private void RegisterConnectionEvent()
    {
        void HoldAndGetEndpoints(LiteNetLib.NetPeer peer)
        {
            CatchExceptionsTo(Logger, () =>
            {
                var package = new Package
                {
                    Route = "/connection/endpoints/hold-and-get",
                    Body = new Dictionary<string, object>
                    {
                        ["Endpoints"] = EndpointsStorage.LocalEndpoints.Select(_ => _.EndpointData)
                    }
                };
                SendPackage(package, peer, DeliveryMethod.ReliableSequenced);
                Listener.PeerConnectedEvent -= HoldAndGetEndpoints;
            });
        }

        void HoldEndpoints(LiteNetLib.NetPeer fromPeer, NetPacketReader dataReader, DeliveryMethod deliveryMethod)
        {
            CatchExceptionsTo(Logger, () =>
            {
                var package = new Package
                {
                    Serialized = dataReader.PeekString(),
                    Schema = DefaultPackageSchema 
                };
                DeserializationMiddleware.Process(package);
                if (package.Route != "/connection/endpoints/hold") return;

                var jsonEndpoints = package.Body!["Endpoints"].ToString()!;
                var endpoints = JsonConvert.DeserializeObject<IList<Endpoint>>(jsonEndpoints)!;
                EndpointsStorage.RemoteEndpoints[fromPeer.Id] = endpoints;
                Listener.NetworkReceiveEvent -= HoldEndpoints;
            });
        }

        Listener.PeerConnectedEvent += HoldAndGetEndpoints;
        Listener.NetworkReceiveEvent += HoldEndpoints;
    }
}