using LiteNetLib;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Kolyhalov.UdpFramework;

public class ServerUdpFramework : UdpFramework
{
    private readonly ServerConfiguration _serverConfiguration;

    public ServerUdpFramework(ServerConfiguration serverConfiguration, ILogger logger,
        IEndpointsInvoker endpointsInvoker, EventBasedNetListener listener) : base(logger, endpointsInvoker, listener)
    {
        _serverConfiguration = serverConfiguration;
    }

    public override void Run()
    {
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
        
        NetManager.Start(_serverConfiguration.Port);
        
        StartListen(_serverConfiguration.Framerate);
    }

    private void RegisterConnectionEvent()
    {
        void HoldAndGetEndpoints(LiteNetLib.NetPeer fromPeer, NetPacketReader dataReader, DeliveryMethod deliveryMethod)
        {
            string jsonPackage = dataReader.PeekString();
            Package package = JsonConvert.DeserializeObject<Package>(jsonPackage)!;
            if (package.Route == "/connection/endpoints/hold-and-get-endpoints")
            {
                RemoteEndpoints[fromPeer.Id] =
                    JsonConvert.DeserializeObject<List<Endpoint>>(package.Body["Endpoints"].ToString()!)!;

                Package responsePackage = new Package
                {
                    Route = "/connection/endpoints/hold-endpoints",
                    Body = new Dictionary<string, object>
                        {["Endpoints"] = LocalEndpoints.Select(endpoint => endpoint.EndpointData)}
                };
                SendMessage(responsePackage, fromPeer.Id, DeliveryMethod.ReliableSequenced);
                Listener.NetworkReceiveEvent -= HoldAndGetEndpoints;
            }
        }

        Listener.NetworkReceiveEvent += HoldAndGetEndpoints;
    }
}