using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.NetPeers;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static Kolyhalov.FatNetLib.ExceptionUtils;
using NetPeer = Kolyhalov.FatNetLib.NetPeers.NetPeer;

namespace Kolyhalov.FatNetLib;

public abstract class NetEventListener
{
    protected readonly EventBasedNetListener Listener;
    private readonly INetworkReceiveEventHandler _receiverEventHandler;
    protected readonly NetManager NetManager;
    protected readonly IList<INetPeer> ConnectedPeers;
    protected readonly IEndpointsStorage EndpointsStorage;
    protected readonly ILogger? Logger;
    private bool _isStop;

    protected NetEventListener(EventBasedNetListener listener,
        INetworkReceiveEventHandler receiverEventHandler,
        NetManager netManager, 
        IList<INetPeer> connectedPeers,
        IEndpointsStorage endpointsStorage, 
        ILogger? logger)
    {
        Listener = listener;
        _receiverEventHandler = receiverEventHandler;
        NetManager = netManager;
        ConnectedPeers = connectedPeers;
        EndpointsStorage = endpointsStorage;
        Logger = logger;
    }

    protected abstract Configuration Configuration { get; }

    public void Run()
    {
        if (_isStop)
            throw new FatNetLibException("FatNetLib finished work");

        StartListen();

        Listener.NetworkReceiveEvent += (peer, reader, method) =>
        {
            CatchExceptionsTo(Logger, () => _receiverEventHandler.Handle(new NetPeer(peer), reader, method));
        };

        Task.Run(() =>
        {
            while (!_isStop)
            {
                CatchExceptionsTo(Logger, () =>
                    {
                        NetManager.PollEvents();
                        Thread.Sleep(Configuration.Framerate.Period);
                    },
                    exceptionMessage: "Polling events failed");
            }
        });
    }

    public void Stop()
    {
        _isStop = true;
        NetManager.Stop();
    }

    protected abstract void StartListen();


    //Todo: remove when we add connection endpoints
    protected void SendMessage(Package package, int peerId, DeliveryMethod deliveryMethod)
    {
        INetPeer peer = ConnectedPeers.Single(netPeer => netPeer.Id == peerId);
        string jsonPackage = JsonConvert.SerializeObject(package);
        var writer = new NetDataWriter();
        writer.Put(jsonPackage);
        peer.Send(writer, deliveryMethod);
    }
}