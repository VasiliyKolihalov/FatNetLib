using Kolyhalov.FatNetLib.Subscribers;
using Kolyhalov.FatNetLib.Timer;
using Kolyhalov.FatNetLib.Wrappers;
using LiteNetLib;
using Microsoft.Extensions.Logging;
using static Kolyhalov.FatNetLib.Utils.ExceptionUtils;
using ConnectionRequest = Kolyhalov.FatNetLib.Wrappers.ConnectionRequest;
using NetPeer = Kolyhalov.FatNetLib.Wrappers.NetPeer;

namespace Kolyhalov.FatNetLib;

public class NetEventListener : INetEventListener
{
    private readonly EventBasedNetListener _listener;
    private readonly INetworkReceiveEventSubscriber _receiverEventSubscriber;
    private readonly IPeerConnectedEventSubscriber _peerConnectedEventSubscriber;
    private readonly IConnectionRequestEventSubscriber _connectionRequestEventSubscriber;
    private readonly IPeerDisconnectedEventSubscriber _peerDisconnectedEventSubscriber;
    private readonly INetManager _netManager;
    private readonly IConnectionStarter _connectionStarter;
    private readonly ITimer _timer;
    private readonly ITimerExceptionHandler _timerExceptionHandler;
    private readonly ILogger _logger;
    private bool _isStop;

    public NetEventListener(
        EventBasedNetListener listener,
        INetworkReceiveEventSubscriber receiverEventSubscriber,
        IPeerConnectedEventSubscriber peerConnectedEventSubscriber,
        IConnectionRequestEventSubscriber connectionRequestEventSubscriber,
        IPeerDisconnectedEventSubscriber peerDisconnectedEventSubscriber,
        INetManager netManager,
        IConnectionStarter connectionStarter,
        ITimer timer,
        ITimerExceptionHandler timerExceptionHandler,
        ILogger logger)
    {
        _listener = listener;
        _receiverEventSubscriber = receiverEventSubscriber;
        _peerConnectedEventSubscriber = peerConnectedEventSubscriber;
        _connectionRequestEventSubscriber = connectionRequestEventSubscriber;
        _peerDisconnectedEventSubscriber = peerDisconnectedEventSubscriber;
        _netManager = netManager;
        _connectionStarter = connectionStarter;
        _timer = timer;
        _timerExceptionHandler = timerExceptionHandler;
        _logger = logger;
    }

    public void Run()
    {
        if (_isStop)
            throw new FatNetLibException("FatNetLib was not designed for reusing after stopping");

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
        _timer.Stop();
        _netManager.Stop();
    }

    private void SubscribeOnPeerConnectedEvent()
    {
        _listener.PeerConnectedEvent += peer =>
            CatchExceptionsTo(_logger, @try: () =>
                _peerConnectedEventSubscriber.Handle(new NetPeer(peer)));
    }

    private void SubscribeOnPeerDisconnectedEvent()
    {
        _listener.PeerDisconnectedEvent += (peer, info) =>
            CatchExceptionsTo(_logger, @try: () =>
                _peerDisconnectedEventSubscriber.Handle(new NetPeer(peer), info));
    }

    private void SubscribeOnNetworkReceiveEvent()
    {
        _listener.NetworkReceiveEvent += (peer, reader, method) =>
            CatchExceptionsTo(_logger, @try: () =>
                Task.Run(() =>
                    CatchExceptionsTo(_logger, @try: () =>
                        _receiverEventSubscriber.Handle(
                            new NetPeer(peer),
                            reader,
                            DeliveryMethodConverter.FromLiteNetLib(method)))));
    }

    private void SubscribeOnConnectionRequestEvent()
    {
        _listener.ConnectionRequestEvent += request =>
            CatchExceptionsTo(_logger, @try: () =>
                _connectionRequestEventSubscriber.Handle(new ConnectionRequest(request)));
    }

    private void RunEventsPolling()
    {
        Task.Run(() =>
        {
            _timer.Start(
                action: () => _netManager.PollEvents(),
                _timerExceptionHandler);
        });
    }
}
