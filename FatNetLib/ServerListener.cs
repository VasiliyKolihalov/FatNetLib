using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.NetPeers;
using LiteNetLib;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static Kolyhalov.FatNetLib.ExceptionUtils;
using NetPeer = Kolyhalov.FatNetLib.NetPeers.NetPeer;


namespace Kolyhalov.FatNetLib;

public class ServerListener : NetEventListener
{
    public ServerListener(EventBasedNetListener listener,
        INetworkReceiveEventHandler receiverEventHandler,
        NetManager netManager,
        IList<INetPeer> connectedPeers,
        IEndpointsStorage endpointsStorage,
        ILogger? logger,
        ServerConfiguration configuration,
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

    protected override ServerConfiguration Configuration { get; }

    protected override void StartListen()
    {
        Listener.PeerConnectedEvent += peer =>
            CatchExceptionsTo(Logger, () =>
                ConnectedPeers.Add(new NetPeer(peer)));
        Listener.PeerDisconnectedEvent += (peer, _) =>
            CatchExceptionsTo(Logger, () =>
                ConnectedPeers.Remove(new NetPeer(peer)));
        Listener.ConnectionRequestEvent += request =>
            CatchExceptionsTo(Logger, () =>
            {
                if (NetManager.ConnectedPeersCount < Configuration.MaxPeers.Value)
                    request.AcceptIfKey(Configuration.ConnectionKey);
                else
                {
                    Logger?.LogError("{IpAddress} could not connect", request.RemoteEndPoint.Address.ScopeId);
                    request.Reject();
                }
            });
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