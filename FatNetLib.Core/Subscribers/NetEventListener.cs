using System.Threading.Tasks;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Timers;
using Kolyhalov.FatNetLib.Core.Wrappers;
using LiteNetLib;
using static Kolyhalov.FatNetLib.Core.Utils.ExceptionUtils;
using ConnectionRequest = Kolyhalov.FatNetLib.Core.Wrappers.ConnectionRequest;
using NetPeer = Kolyhalov.FatNetLib.Core.Wrappers.NetPeer;

namespace Kolyhalov.FatNetLib.Core.Subscribers
{
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
}
