using Kolyhalov.UdpFramework.Configurations;
using Kolyhalov.UdpFramework.Endpoints;
using Kolyhalov.UdpFramework.ResponsePackageMonitors;
using LiteNetLib;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NetPeer = Kolyhalov.UdpFramework.NetPeers.NetPeer;
using static Kolyhalov.UdpFramework.ExceptionUtils;

namespace Kolyhalov.UdpFramework;

public class ClientUdpFramework : UdpFramework
{
    protected override ClientConfiguration Configuration { get; }

    public ClientUdpFramework(ClientConfiguration configuration,
        ILogger? logger,
        IEndpointsStorage endpointsStorage,
        IEndpointsInvoker endpointsInvoker,
        EventBasedNetListener listener,
        IResponsePackageMonitor responsePackageMonitor) : base(logger, endpointsStorage, endpointsInvoker, listener,
        responsePackageMonitor)
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

        RegisterConnectionEvent();

        NetManager.Start();
        NetManager.Connect(Configuration.Address,
            Configuration.Port.Value,
            Configuration.ConnectionKey);

        StartListen();
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
                SendMessage(package, peer.Id, DeliveryMethod.ReliableSequenced);
                Listener.PeerConnectedEvent -= HoldAndGetEndpoints;
            });
        }

        void HoldEndpoints(LiteNetLib.NetPeer fromPeer, NetPacketReader dataReader, DeliveryMethod deliveryMethod)
        {
            CatchExceptionsTo(Logger, () =>
            {
                string jsonPackage = dataReader.PeekString();
                var package = JsonConvert.DeserializeObject<Package>(jsonPackage)!;
                if (package.Route != "/connection/endpoints/hold") return;

                var jsonEndpoints = package.Body!["Endpoints"].ToString()!;
                var endpoints = JsonConvert.DeserializeObject<List<Endpoint>>(jsonEndpoints)!;
                EndpointsStorage.RemoteEndpoints[fromPeer.Id] = endpoints;
                Listener.NetworkReceiveEvent -= HoldEndpoints;
            });
        }

        Listener.PeerConnectedEvent += HoldAndGetEndpoints;
        Listener.NetworkReceiveEvent += HoldEndpoints;
    }
}