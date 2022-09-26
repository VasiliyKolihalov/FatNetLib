using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.LiteNetLibWrappers;
using Kolyhalov.FatNetLib.Subscribers;
using LiteNetLib;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static Kolyhalov.FatNetLib.ExceptionUtils;
using ConnectionRequest = Kolyhalov.FatNetLib.LiteNetLibWrappers.ConnectionRequest;
using NetPeer = Kolyhalov.FatNetLib.LiteNetLibWrappers.NetPeer;


namespace Kolyhalov.FatNetLib;

public class ServerListener : NetEventListener
{
    private readonly IConnectionRequestEventSubscriber _connectionRequestEventSub;
    protected override ServerConfiguration Configuration { get; }
    
    public ServerListener(EventBasedNetListener listener,
        INetworkReceiveEventSubscriber receiverEventSub,
        INetManager netManager,
        IList<INetPeer> connectedPeers,
        IEndpointsStorage endpointsStorage,
        ILogger? logger,
        ServerConfiguration configuration,
        PackageSchema defaultPackageSchema, 
        IConnectionRequestEventSubscriber connectionRequestEventSub) : base(listener,
        receiverEventSub,
        netManager,
        connectedPeers,
        endpointsStorage,
        logger,
        defaultPackageSchema)
    {
        Configuration = configuration;
        _connectionRequestEventSub = connectionRequestEventSub;
    }

    protected override void StartListen()
    {
        Listener.PeerConnectedEvent += peer =>
            CatchExceptionsTo(Logger, () =>
                ConnectedPeers.Add(new NetPeer(peer)));
        Listener.PeerDisconnectedEvent += (peer, _) =>
            CatchExceptionsTo(Logger, () =>
                ConnectedPeers.Remove(new NetPeer(peer)));
        Listener.ConnectionRequestEvent += request =>
            CatchExceptionsTo(Logger, () => _connectionRequestEventSub.Handle(new ConnectionRequest(request)));
        Listener.PeerDisconnectedEvent += (peer, _) =>
            CatchExceptionsTo(Logger, () =>
                EndpointsStorage.RemoteEndpoints.Remove(peer.Id));
        RegisterConnectionEvent();

        NetManager.Start(Configuration.Port.Value);
    }

    private void RegisterConnectionEvent()
    {
        void HoldAndGetEndpoints(LiteNetLib.NetPeer fromPeer, NetPacketReader dataReader, DeliveryMethod deliveryMethod)
        {
            CatchExceptionsTo(Logger, () =>
            {
                var package = new Package
                {
                    Serialized = dataReader.PeekString(),
                    Schema = DefaultPackageSchema
                };
                DeserializationMiddleware.Process(package);

                if (package.Route != "/connection/endpoints/hold-and-get") return;

                var jsonEndpoints = package.Body!["Endpoints"].ToString()!;
                var endpoints = JsonConvert.DeserializeObject<IList<Endpoint>>(jsonEndpoints)!;
                EndpointsStorage.RemoteEndpoints[fromPeer.Id] = endpoints;

                var responsePackage = new Package
                {
                    Route = "/connection/endpoints/hold",
                    Body = new Dictionary<string, object>
                    {
                        ["Endpoints"] = EndpointsStorage.LocalEndpoints.Select(_ => _.EndpointData)
                    }
                };
                SendPackage(responsePackage, fromPeer, DeliveryMethod.ReliableSequenced);
                Listener.NetworkReceiveEvent -= HoldAndGetEndpoints;
            });
        }

        Listener.NetworkReceiveEvent += HoldAndGetEndpoints;
    }
}