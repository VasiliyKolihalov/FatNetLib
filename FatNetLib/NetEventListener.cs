using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Subscribers;
using Kolyhalov.FatNetLib.Wrappers;
using LiteNetLib;
using Microsoft.Extensions.Logging;
using static Kolyhalov.FatNetLib.Utils.ExceptionUtils;
using ConnectionRequest = Kolyhalov.FatNetLib.Wrappers.ConnectionRequest;
using NetPeer = Kolyhalov.FatNetLib.Wrappers.NetPeer;

namespace Kolyhalov.FatNetLib;

public class NetEventListener
{
    private readonly EventBasedNetListener _listener;
    private readonly INetworkReceiveEventSubscriber _receiverEventSubscriber;
    private readonly IPeerConnectedEventSubscriber _peerConnectedEventSubscriber;
    private readonly IConnectionRequestEventSubscriber _connectionRequestEventSubscriber;
    private readonly IPeerDisconnectedEventSubscriber _peerDisconnectedEventSubscriber;
    private readonly INetManager _netManager;
    private readonly IConnectionStarter _connectionStarter;
    private readonly Configuration _configuration;
    private readonly ILogger? _logger;
    private bool _isStop;

    public NetEventListener(EventBasedNetListener listener,
        INetworkReceiveEventSubscriber receiverEventSubscriber,
        IPeerConnectedEventSubscriber peerConnectedEventSubscriber,
        IConnectionRequestEventSubscriber connectionRequestEventSubscriber,
        IPeerDisconnectedEventSubscriber peerDisconnectedEventSubscriber,
        INetManager netManager,
        IConnectionStarter connectionStarter,
        Configuration configuration,
        ILogger? logger)
    {
        _listener = listener;
        _receiverEventSubscriber = receiverEventSubscriber;
        _peerConnectedEventSubscriber = peerConnectedEventSubscriber;
        _connectionRequestEventSubscriber = connectionRequestEventSubscriber;
        _peerDisconnectedEventSubscriber = peerDisconnectedEventSubscriber;
        _netManager = netManager;
        _connectionStarter = connectionStarter;
        _configuration = configuration;
        _logger = logger;
    }

    public void Run()
    {
        if (_isStop)
            throw new FatNetLibException("FatNetLib finished work");

        SubscribeOnPeerConnectedEvent();
        SubscribeOnPeerDisconnectedEvent();
        SubscribeOnNetworkReceiveEvent();
        SubscribeOnConnectionRequestEvent();

        _connectionStarter.StartConnection();
        RunEventsPolling();
    }

    public void Stop()
    {
        _isStop = true;
        _netManager.Stop();
    }

    private void SubscribeOnPeerConnectedEvent()
    {
        _listener.PeerConnectedEvent += peer =>
            CatchExceptionsTo(_logger,
                @try: () =>
                    _peerConnectedEventSubscriber.Handle(new NetPeer(peer)));
    }

    private void SubscribeOnPeerDisconnectedEvent()
    {
        _listener.PeerDisconnectedEvent += (peer, info) =>
            CatchExceptionsTo(_logger,
                @try: () =>
                    _peerDisconnectedEventSubscriber.Handle(new NetPeer(peer), info));
    }

    private void SubscribeOnNetworkReceiveEvent()
    {
        _listener.NetworkReceiveEvent += (peer, reader, method) =>
            CatchExceptionsTo(_logger,
                @try: () =>
                    Task.Run(() =>
                        CatchExceptionsTo(_logger,
                            @try: () =>
                                _receiverEventSubscriber.Handle(new NetPeer(peer), reader, method))));
    }

    private void SubscribeOnConnectionRequestEvent()
    {
        _listener.ConnectionRequestEvent += request =>
            CatchExceptionsTo(_logger,
                @try: () =>
                    _connectionRequestEventSubscriber.Handle(new ConnectionRequest(request)));
    }

    private void RunEventsPolling()
    {
        Task.Run(() =>
        {
            while (!_isStop)
            {
                CatchExceptionsTo(_logger, @try: () =>
                    {
                        _netManager.PollEvents();
                        Thread.Sleep(_configuration.Framerate.Period);
                    },
                    message: "Events polling failed");
            }
        });
    }
}