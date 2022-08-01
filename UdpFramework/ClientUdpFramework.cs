using LiteNetLib;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Kolyhalov.UdpFramework;

public class ClientUdpFramework : UdpFramework
{
    private readonly ClientConfiguration _clientConfiguration;

    public ClientUdpFramework(ClientConfiguration clientConfiguration,
        ILogger logger,
        EndpointsStorage endpointsStorage,
        IEndpointsInvoker endpointsInvoker,
        EventBasedNetListener listener) : base(logger, endpointsStorage, endpointsInvoker, listener)
    {
        _clientConfiguration = clientConfiguration;
    }

    public override void Run()
    {
        RegisterConnectionEvent();

        NetManager.Start();
        NetManager.Connect(_clientConfiguration.Address, _clientConfiguration.Port, _clientConfiguration.ConnectionKey);

        StartListen(_clientConfiguration.Framerate);
    }

    private void RegisterConnectionEvent()
    {
        void HoldAndGetEndpoints(LiteNetLib.NetPeer peer)
        {
            Package package = new Package
            {
                Route = "/connection/endpoints/hold-and-get-endpoints",
                Body = new Dictionary<string, object> {["Endpoints"] = EndpointsStorage.GetLocalEndpointsData()}
            };
            SendMessage(package, peer.Id, DeliveryMethod.ReliableSequenced);
            Listener.PeerConnectedEvent -= HoldAndGetEndpoints;
        }

        void HoldEndpoints(LiteNetLib.NetPeer fromPeer, NetPacketReader dataReader, DeliveryMethod deliveryMethod)
        {
            string jsonPackage = dataReader.PeekString();
            Package package = JsonConvert.DeserializeObject<Package>(jsonPackage)!;
            if (package.Route == "/connection/endpoints/hold-endpoints")
            {
                string jsonEndpoints = package.Body!["Endpoints"].ToString()!;
                List<Endpoint> endpoints = JsonConvert.DeserializeObject<List<Endpoint>>(jsonEndpoints)!;
                EndpointsStorage.AddRemoteEndpoints(fromPeer.Id, endpoints);
                Listener.NetworkReceiveEvent -= HoldEndpoints;
            }
        }

        Listener.PeerConnectedEvent += HoldAndGetEndpoints;
        Listener.NetworkReceiveEvent += HoldEndpoints;
    }
}