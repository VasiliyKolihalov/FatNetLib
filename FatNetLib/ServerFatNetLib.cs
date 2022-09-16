using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Middlewares;
using Kolyhalov.FatNetLib.ResponsePackageMonitors;
using LiteNetLib;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NetPeer = Kolyhalov.FatNetLib.NetPeers.NetPeer;
using static Kolyhalov.FatNetLib.ExceptionUtils;

namespace Kolyhalov.FatNetLib;

public class ServerFatNetLib : FatNetLib
{
    protected override ServerConfiguration Configuration { get; }

    public ServerFatNetLib(ServerConfiguration configuration,
        ILogger? logger,
        IEndpointsStorage endpointsStorage,
        IEndpointsInvoker endpointsInvoker,
        EventBasedNetListener listener,
        IResponsePackageMonitor responsePackageMonitor,
        IMiddlewaresRunner sendingMiddlewaresRunner,
        IMiddlewaresRunner receivingMiddlewaresRunner) : base(logger,
        endpointsStorage,
        endpointsInvoker,
        listener,
        responsePackageMonitor,
        sendingMiddlewaresRunner,
        receivingMiddlewaresRunner)
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
                EndpointsStorage.RemoteEndpoints.Remove(peer.Id));

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
                SendMessage(responsePackage, fromPeer.Id, DeliveryMethod.ReliableSequenced);
                Listener.NetworkReceiveEvent -= HoldAndGetEndpoints;
            });
        }

        Listener.NetworkReceiveEvent += HoldAndGetEndpoints;
    }
}