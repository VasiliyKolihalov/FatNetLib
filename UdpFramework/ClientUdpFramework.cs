using LiteNetLib;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Kolyhalov.UdpFramework;

public class ClientUdpFramework : UdpFramework
{
    private readonly ClientConfiguration _clientConfiguration;

    public ClientUdpFramework(ClientConfiguration clientConfiguration,
        ILogger? logger,
        IEndpointsStorage endpointsStorage,
        IEndpointsInvoker endpointsInvoker,
        EventBasedNetListener listener) : base(logger, endpointsStorage, endpointsInvoker, listener)
    {
        _clientConfiguration = clientConfiguration;
    }

    public override void Run()
    {
        Listener.PeerConnectedEvent += peer => ConnectedPeers.Add(new NetPeer(peer));
        Listener.PeerDisconnectedEvent += (peer, _) => ConnectedPeers.Remove(new NetPeer(peer));

        RegisterConnectionEvent();

        NetManager.Start();
        NetManager.Connect(_clientConfiguration.Address, 
            _clientConfiguration.Port.Value,
            _clientConfiguration.ConnectionKey);

        StartListen(_clientConfiguration.Framerate.Value);
    }

    private void RegisterConnectionEvent()
    {
        void HoldAndGetEndpoints(LiteNetLib.NetPeer peer)
        {
            var package = new Package
            {
                Route = "/connection/endpoints/hold-and-get",
                Body = new Dictionary<string, object> {["Endpoints"] = EndpointsStorage.GetLocalEndpointsData()}
            };
            SendMessage(package, peer.Id, DeliveryMethod.ReliableSequenced);
            Listener.PeerConnectedEvent -= HoldAndGetEndpoints;
        }

        void HoldEndpoints(LiteNetLib.NetPeer fromPeer, NetPacketReader dataReader, DeliveryMethod deliveryMethod)
        {
            string jsonPackage = dataReader.PeekString();
            var package = JsonConvert.DeserializeObject<Package>(jsonPackage)!;
            if (package.Route != "/connection/endpoints/hold") return;

            var jsonEndpoints = package.Body!["Endpoints"].ToString()!;
            var endpoints = JsonConvert.DeserializeObject<List<Endpoint>>(jsonEndpoints)!;
            EndpointsStorage.AddRemoteEndpoints(fromPeer.Id, endpoints);
            Listener.NetworkReceiveEvent -= HoldEndpoints;
        }

        Listener.PeerConnectedEvent += HoldAndGetEndpoints;
        Listener.NetworkReceiveEvent += HoldEndpoints;
    }
}