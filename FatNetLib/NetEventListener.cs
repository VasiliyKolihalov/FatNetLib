using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Subscribers;
using Kolyhalov.FatNetLib.Wrappers;
using LiteNetLib;
using Microsoft.Extensions.Logging;
using static Kolyhalov.FatNetLib.Utils.ExceptionUtils;
using NetPeer = Kolyhalov.FatNetLib.Wrappers.NetPeer;

namespace Kolyhalov.FatNetLib;

public abstract class NetEventListener
{
    protected readonly EventBasedNetListener Listener;
    private readonly INetworkReceiveEventSubscriber _receiverEventSubscriber;
    private readonly IPeerConnectedEventSubscriber _peerConnectedEventSubscriber;
    protected readonly INetManager NetManager;
    private readonly IList<INetPeer> _connectedPeers;
    protected readonly IEndpointsStorage EndpointsStorage;
    protected readonly ILogger? Logger;
    private bool _isStop;

    protected NetEventListener(EventBasedNetListener listener,
        INetworkReceiveEventSubscriber receiverEventSubscriber,
        IPeerConnectedEventSubscriber peerConnectedEventSubscriber,
        INetManager netManager,
        IList<INetPeer> connectedPeers,
        IEndpointsStorage endpointsStorage,
        ILogger? logger)
    {
        Listener = listener;
        _receiverEventSubscriber = receiverEventSubscriber;
        _peerConnectedEventSubscriber = peerConnectedEventSubscriber;
        NetManager = netManager;
        _connectedPeers = connectedPeers;
        EndpointsStorage = endpointsStorage;
        Logger = logger;
    }

    protected abstract Configuration Configuration { get; }

    public void Run()
    {
        if (_isStop)
            throw new FatNetLibException("FatNetLib finished work");

        SubscribeOnPeerConnectedEvent();
        SubscribeOnPeerDisconnectedEvent();

        StartListen();

        // Todo: figure out why placing this SubscribeOn() before StartListen() causes exception in ticket #60
        SubscribeOnNetworkReceiveEvent();

        RunEventsPolling();
    }

    private void RunEventsPolling()
    {
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

    private void SubscribeOnPeerConnectedEvent()
    {
        Listener.PeerConnectedEvent += peer =>
            CatchExceptionsTo(Logger,
                @try: () =>
                    _peerConnectedEventSubscriber.Handle(new NetPeer(peer)));
    }

    private void SubscribeOnPeerDisconnectedEvent()
    {
        Listener.PeerDisconnectedEvent += (peer, _) =>
            CatchExceptionsTo(Logger,
                @try: () =>
                    _connectedPeers.Remove(new NetPeer(peer)));
    }

    private void SubscribeOnNetworkReceiveEvent()
    {
        Listener.NetworkReceiveEvent += (peer, reader, method) =>
            CatchExceptionsTo(Logger,
                @try: () => Task.Run(() => _receiverEventSubscriber.Handle(new NetPeer(peer), reader, method)));
    }

    public void Stop()
    {
        _isStop = true;
        NetManager.Stop();
    }

    protected abstract void StartListen();
}