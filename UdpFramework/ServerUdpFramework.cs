using LiteNetLib;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Kolyhalov.UdpFramework;

public class ServerUdpFramework : UdpFramework
{
    private readonly ServerConfiguration _serverConfiguration;

    public ServerUdpFramework(ServerConfiguration serverConfiguration,
        ILogger logger,
        EndpointsStorage endpointsStorage,
        IEndpointsInvoker endpointsInvoker,
        EventBasedNetListener listener) : base(logger, endpointsStorage, endpointsInvoker, listener)
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

        Listener.PeerDisconnectedEvent += (peer, _) => { EndpointsStorage.RemoveRemoteEndpoints(peer.Id); };

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
                string jsonEndpoints = package.Body!["Endpoints"].ToString()!;
                List<Endpoint> endpoints = JsonConvert.DeserializeObject<List<Endpoint>>(jsonEndpoints)!;
                EndpointsStorage.AddRemoteEndpoints(fromPeer.Id,endpoints);

                Package responsePackage = new Package
                {
                    Route = "/connection/endpoints/hold-endpoints",
                    Body = new Dictionary<string, object> {["Endpoints"] = EndpointsStorage.GetLocalEndpointsData()}
                };
                SendMessage(responsePackage, fromPeer.Id, DeliveryMethod.ReliableSequenced);
                Listener.NetworkReceiveEvent -= HoldAndGetEndpoints;
            }
        }

        Listener.NetworkReceiveEvent += HoldAndGetEndpoints;
    }
}