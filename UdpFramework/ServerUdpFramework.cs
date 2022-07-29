using System.Reflection.Metadata.Ecma335;
using LiteNetLib;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Kolyhalov.UdpFramework;

public class ServerUdpFramework : UdpFramework
{
    private readonly ServerConfiguration _serverConfiguration;

    public ServerUdpFramework(ServerConfiguration serverConfiguration, ILogger logger,
        IEndpointsHandler endpointsHandler) : base(logger, endpointsHandler)
    {
        _serverConfiguration = serverConfiguration;
    }

    public override void Run()
    {
        NetManager.Start(_serverConfiguration.Port);

        Listener.ConnectionRequestEvent += request =>
        {
            if (NetManager.ConnectedPeersCount < _serverConfiguration.MaxPeersCount)
                request.AcceptIfKey(_serverConfiguration.ConnectionKey);
            else
            {
                Logger.LogError($"{request.RemoteEndPoint.Address.ScopeId} could not connect");
                request.Reject();
            }
        };

        Listener.PeerDisconnectedEvent += (peer, _) => { RemoteEndpoints.Remove(peer.Id); };

        RegisterConnectionEvent();

        StartListen(_serverConfiguration.Framerate);
    }

    private void RegisterConnectionEvent()
    {
        void HoldAndGetEndpoints(NetPeer fromPeer, NetPacketReader dataReader, DeliveryMethod deliveryMethod)
        {
            string jsonPackage = dataReader.PeekString();
            Package package = JsonConvert.DeserializeObject<Package>(jsonPackage)!;
            if (package.Route == "/connection/hold-and-get-endpoints")
            {
                RemoteEndpoints[fromPeer.Id] =
                    JsonConvert.DeserializeObject<List<Endpoint>>(package.Body["Endpoints"].ToString()!)!;

                Package responsePackage = new Package
                {
                    Route = "/connection/hold-endpoints",
                    Body = new Dictionary<string, object>
                        {["Endpoints"] = LocalEndpoints.Select(endpoint => endpoint.EndpointData)}
                };
                SendMessage(responsePackage, new NetPeerShell(fromPeer), DeliveryMethod.ReliableSequenced);
                Listener.NetworkReceiveEvent -= HoldAndGetEndpoints;
            }
        }

        Listener.NetworkReceiveEvent += HoldAndGetEndpoints;
    }
}