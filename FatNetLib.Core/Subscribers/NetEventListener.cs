using System.Threading.Tasks;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Timers;
using Kolyhalov.FatNetLib.Core.Wrappers;
using LiteNetLib;
using static Kolyhalov.FatNetLib.Core.Controllers.RouteConstants.Routes;
using static Kolyhalov.FatNetLib.Core.Controllers.RouteConstants.Routes.Events;
using static Kolyhalov.FatNetLib.Core.Utils.ExceptionUtils;
using ConnectionRequest = Kolyhalov.FatNetLib.Core.Wrappers.ConnectionRequest;
using NetPeer = Kolyhalov.FatNetLib.Core.Wrappers.NetPeer;

namespace Kolyhalov.FatNetLib.Core.Subscribers
{
    public class NetEventListener : INetEventListener
    {
        private readonly EventBasedNetListener _listener;
        private readonly ICourier _courier;
        private readonly INetManager _netManager;
        private readonly IConnectionStarter _connectionStarter;
        private readonly ITimer _timer;
        private readonly ITimerExceptionHandler _timerExceptionHandler;
        private readonly ILogger _logger;
        private bool _isStop;

        public NetEventListener(
            EventBasedNetListener listener,
            ICourier courier,
            INetManager netManager,
            IConnectionStarter connectionStarter,
            ITimer timer,
            ITimerExceptionHandler timerExceptionHandler,
            ILogger logger)
        {
            _listener = listener;
            _courier = courier;
            _netManager = netManager;
            _connectionStarter = connectionStarter;
            _timer = timer;
            _timerExceptionHandler = timerExceptionHandler;
            _logger = logger;
        }

        public void Run()
        {
            // Todo: restrict running twice
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
                {
                    _courier.EmitEvent(new Package
                    {
                        Route = PeerConnected,
                        Body = new NetPeer(peer)
                    });
                });
        }

        private void SubscribeOnPeerDisconnectedEvent()
        {
            _listener.PeerDisconnectedEvent += (peer, info) =>
                CatchExceptionsTo(_logger, @try: () =>
                {
                    _courier.EmitEvent(new Package
                    {
                        Route = PeerDisconnected,
                        Body = new PeerDisconnectedBody
                        {
                            NetPeer = new NetPeer(peer),
                            DisconnectInfo = info
                        }
                    });
                });
        }

        private void SubscribeOnNetworkReceiveEvent()
        {
            _listener.NetworkReceiveEvent += (peer, reader, method) =>
                CatchExceptionsTo(_logger, @try: () =>
                    Task.Run(() =>
                        CatchExceptionsTo(_logger, @try: () =>
                        {
                            _courier.EmitEvent(new Package
                            {
                                Route = NetworkReceived,
                                Body = new NetworkReceiveBody
                                {
                                    NetPeer = new NetPeer(peer),
                                    PacketReader = reader,
                                    Reliability = DeliveryMethodConverter.FromLiteNetLib(method)
                                }
                            });
                        })));
        }

        private void SubscribeOnConnectionRequestEvent()
        {
            _listener.ConnectionRequestEvent += request =>
                CatchExceptionsTo(_logger, @try: () =>
                {
                    _courier.EmitEvent(new Package
                    {
                        Route = Events.ConnectionRequest,
                        Body = new ConnectionRequest(request)
                    });
                });
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
