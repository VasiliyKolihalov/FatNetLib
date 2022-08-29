using Kolyhalov.UdpFramework.Configurations;
using Kolyhalov.UdpFramework.Endpoints;
using Kolyhalov.UdpFramework.ResponsePackageMonitors;
using LiteNetLib;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NetPeer = Kolyhalov.UdpFramework.NetPeers.NetPeer;
using static Kolyhalov.UdpFramework.ExceptionUtils;

namespace Kolyhalov.UdpFramework;

public class ServerUdpFramework : UdpFramework
{
    protected override ServerConfiguration Configuration { get; }

    public ServerUdpFramework(ServerConfiguration configuration,
        ILogger? logger,
        IEndpointsStorage endpointsStorage,
        IEndpointsInvoker endpointsInvoker,
        EventBasedNetListener listener,
        IResponsePackageMonitor responsePackageMonitor) : base(logger, endpointsStorage, endpointsInvoker,
            listener, responsePackageMonitor)
    {
        Configuration = configuration;
    }

    public override void Run()
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
                    Logger?.LogError($"{request.RemoteEndPoint.Address.ScopeId} could not connect");
                    request.Reject();
                }
            });

        Listener.PeerDisconnectedEvent += (peer, _) =>
            CatchExceptionsTo(Logger, () =>
                EndpointsStorage.RemoveRemoteEndpoints(peer.Id));

        RegisterConnectionEvent();

        NetManager.Start(Configuration.Port.Value);

        StartListen();
    }

    private void RegisterConnectionEvent()
    {
        void HoldAndGetEndpoints(LiteNetLib.NetPeer fromPeer, NetPacketReader dataReader, DeliveryMethod deliveryMethod)
        {
            CatchExceptionsTo(Logger, () =>
            {
                string jsonPackage = dataReader.PeekString();
                var package = JsonConvert.DeserializeObject<Package>(jsonPackage)!;
                if (package.Route != "/connection/endpoints/hold-and-get") return;

                var jsonEndpoints = package.Body!["Endpoints"].ToString()!;
                var endpoints = JsonConvert.DeserializeObject<List<Endpoint>>(jsonEndpoints)!;
                EndpointsStorage.AddRemoteEndpoints(fromPeer.Id, endpoints);

                var responsePackage = new Package
                {
                    Route = "/connection/endpoints/hold",
                    Body = new Dictionary<string, object> { ["Endpoints"] = EndpointsStorage.GetLocalEndpointsData() }
                };
                SendMessage(responsePackage, fromPeer.Id, DeliveryMethod.ReliableSequenced);
                Listener.NetworkReceiveEvent -= HoldAndGetEndpoints;
            });
        }

        Listener.NetworkReceiveEvent += HoldAndGetEndpoints;
    }
}