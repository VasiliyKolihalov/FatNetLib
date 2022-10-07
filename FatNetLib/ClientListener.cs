using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Microtypes;
using Kolyhalov.FatNetLib.Subscribers;
using Kolyhalov.FatNetLib.Wrappers;
using LiteNetLib;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static Kolyhalov.FatNetLib.Utils.ExceptionUtils;
using NetPeer = Kolyhalov.FatNetLib.Wrappers.NetPeer;

namespace Kolyhalov.FatNetLib;

public class ClientListener : NetEventListener
{
    private readonly string _protocolVersion;
    protected override ClientConfiguration Configuration { get; }

    public ClientListener(EventBasedNetListener listener,
        INetworkReceiveEventSubscriber receiverEventSubscriber,
        INetManager netManager,
        IList<INetPeer> connectedPeers,
        IEndpointsStorage endpointsStorage,
        ILogger? logger,
        ClientConfiguration configuration,
        PackageSchema defaultPackageSchema,
        IProtocolVersionProvider protocolVersionProvider) : base(listener,
        receiverEventSubscriber,
        netManager,
        connectedPeers,
        endpointsStorage,
        logger,
        defaultPackageSchema)
    {
        Configuration = configuration;
        _protocolVersion = protocolVersionProvider.Get();
    }

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
            key: _protocolVersion);
    }

    private void RegisterConnectionEvent()
    {
        void HoldAndGetEndpoints(LiteNetLib.NetPeer peer)
        {
            CatchExceptionsTo(Logger, () =>
            {
                var package = new Package
                {
                    Route = new Route("/connection/endpoints/hold-and-get"),
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
                if (!package.Route!.Equals(new Route("/connection/endpoints/hold"))) return;

                var jsonEndpoints = package.Body!["Endpoints"].ToString()!;
                var endpoints =
                    JsonConvert.DeserializeObject<IList<Endpoint>>(jsonEndpoints, Serializer.Converters.ToArray())!;
                EndpointsStorage.RemoteEndpoints[fromPeer.Id] = endpoints;
                Listener.NetworkReceiveEvent -= HoldEndpoints;
            });
        }

        Listener.PeerConnectedEvent += HoldAndGetEndpoints;
        Listener.NetworkReceiveEvent += HoldEndpoints;
    }
}