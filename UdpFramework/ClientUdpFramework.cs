using LiteNetLib;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Kolyhalov.UdpFramework;

public class ClientUdpFramework : UdpFramework
{
    private readonly ClientConfiguration _clientConfiguration;

    public ClientUdpFramework(ClientConfiguration clientConfiguration, ILogger logger,
        IEndpointsInvoker endpointsInvoker, EventBasedNetListener listener) : base(logger, endpointsInvoker, listener)
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
                Body = new Dictionary<string, object>
                    {["Endpoints"] = LocalEndpoints.Select(endpoint => endpoint.EndpointData)}
            };
            SendMessage(package, peer.Id, DeliveryMethod.ReliableSequenced);
            Listener.PeerConnectedEvent -= HoldAndGetEndpoints;
        }

        void GetEndpoints(LiteNetLib.NetPeer fromPeer, NetPacketReader dataReader, DeliveryMethod deliveryMethod)
        {
            string jsonPackage = dataReader.PeekString();
            Package package = JsonConvert.DeserializeObject<Package>(jsonPackage)!;
            if (package.Route == "/connection/endpoints/hold-endpoints")
            {
                RemoteEndpoints[fromPeer.Id] =
                    JsonConvert.DeserializeObject<List<Endpoint>>(package.Body["Endpoints"].ToString()!)!;
                Listener.NetworkReceiveEvent -= GetEndpoints;
            }
        }

        Listener.PeerConnectedEvent += HoldAndGetEndpoints;
        Listener.NetworkReceiveEvent += GetEndpoints;
    }
}