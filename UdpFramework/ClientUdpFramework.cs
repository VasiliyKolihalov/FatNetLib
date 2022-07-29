using LiteNetLib;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Kolyhalov.UdpFramework;

public class ClientUdpFramework : UdpFramework
{
    private readonly ClientConfiguration _clientConfiguration;

    public ClientUdpFramework(ClientConfiguration clientConfiguration, ILogger logger,
        IEndpointsHandler endpointsHandler) : base(logger, endpointsHandler)
    {
        _clientConfiguration = clientConfiguration;
    }

    public override void Run()
    {
        NetManager.Start();
        NetManager.Connect(_clientConfiguration.Address, _clientConfiguration.Port, _clientConfiguration.ConnectionKey);

        RegisterConnectionEvent();

        StartListen(_clientConfiguration.Framerate);
    }

    private void RegisterConnectionEvent()
    {
        void SendEndpoints(NetPeer peer)
        {
            Package endpointsRequest = new Package
            {
                Route = "/connection/hold-and-get-endpoints",
                Body = new Dictionary<string, object>
                    {["Endpoints"] = LocalEndpoints.Select(endpoint => endpoint.EndpointData)}
            };
            SendMessage(endpointsRequest, new NetPeerShell(peer), DeliveryMethod.ReliableSequenced);
            Listener.PeerConnectedEvent -= SendEndpoints;
        }

        void GetEndpoints(NetPeer fromPeer, NetPacketReader dataReader, DeliveryMethod deliveryMethod)
        {
            string jsonPackage = dataReader.PeekString();
            Package package = JsonConvert.DeserializeObject<Package>(jsonPackage)!;
            if (package.Route == "/connection/hold-endpoints")
            {
                RemoteEndpoints[fromPeer.Id] =
                    JsonConvert.DeserializeObject<List<Endpoint>>(package.Body["Endpoints"].ToString()!)!;
                Listener.NetworkReceiveEvent -= GetEndpoints;
            }
        }

        Listener.PeerConnectedEvent += SendEndpoints;
        Listener.NetworkReceiveEvent += GetEndpoints;
    }
}