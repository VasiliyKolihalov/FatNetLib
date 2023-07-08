using System.Threading.Tasks;
using Kolyhalov.FatNetLib.Core.Couriers;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Timers;
using Kolyhalov.FatNetLib.Core.Wrappers;
using LiteNetLib;
using static Kolyhalov.FatNetLib.Core.Constants.RouteConstants.Routes;
using static Kolyhalov.FatNetLib.Core.Constants.RouteConstants.Routes.Events;
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
            SubscribeOnNetworkErrorEvent();
            SubscribeOnNetworkReceiveUnconnectedEventEvent();
            SubscribeOnNetworkLatencyUpdateEventEvent();
            SubscribeOnDeliveryEventEvent();
            SubscribeOnNtpResponseEventEvent();

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
                _courier.EmitEventAsync(new Package
                {
                    Route = PeerConnected,
                    Body = new NetPeer(peer)
                }).ContinueWithLogException(_logger, "Failed to handle PeerConnectedEvent");
        }

        private void SubscribeOnPeerDisconnectedEvent()
        {
            _listener.PeerDisconnectedEvent += (peer, info) =>
                _courier.EmitEventAsync(new Package
                {
                    Route = PeerDisconnected,
                    Body = new PeerDisconnectedBody
                    {
                        Peer = new NetPeer(peer),
                        DisconnectInfo = info
                    }
                }).ContinueWithLogException(_logger, "Failed to handle PeerDisconnectedEvent");
        }

        private void SubscribeOnNetworkReceiveEvent()
        {
            _listener.NetworkReceiveEvent += (peer, reader, method) =>
                _courier.EmitEventAsync(new Package
                {
                    Route = NetworkReceived,
                    Body = new NetworkReceiveBody
                    {
                        Peer = new NetPeer(peer),
                        DataReader = reader,
                        Reliability = DeliveryMethodConverter.FromLiteNetLib(method)
                    }
                }).ContinueWithLogException(_logger, "Failed to handle NetworkReceiveEvent");
        }

        private void SubscribeOnConnectionRequestEvent()
        {
            _listener.ConnectionRequestEvent += request =>
                _courier.EmitEventAsync(new Package
                {
                    Route = Events.ConnectionRequest,
                    Body = new ConnectionRequest(request)
                }).ContinueWithLogException(_logger, "Failed to handle ConnectionRequestEvent");
        }

        private void SubscribeOnNetworkErrorEvent()
        {
            _listener.NetworkErrorEvent += (remoteEndPoint, socketError) =>
                _courier.EmitEventAsync(new Package
                {
                    Route = NetworkError,
                    Body = new NetworkErrorBody
                    {
                        IPEndPoint = remoteEndPoint,
                        SocketError = socketError
                    }
                }).ContinueWithLogException(_logger, "Failed to handle NetworkErrorEvent");
        }

        private void SubscribeOnNetworkReceiveUnconnectedEventEvent()
        {
            _listener.NetworkReceiveUnconnectedEvent += (remoteEndPoint, reader, messageType) =>
                _courier.EmitEventAsync(new Package
                {
                    Route = NetworkReceiveUnconnected,
                    Body = new NetworkReceiveUnconnectedBody
                    {
                        IPEndPoint = remoteEndPoint,
                        NetPacketReader = reader,
                        UnconnectedMessageType = messageType
                    }
                }).ContinueWithLogException(_logger, "Failed to handle NetworkReceiveUnconnectedEvent");
        }

        private void SubscribeOnNetworkLatencyUpdateEventEvent()
        {
            _listener.NetworkLatencyUpdateEvent += (peer, latency) =>
                _courier.EmitEventAsync(new Package
                {
                    Route = NetworkLatencyUpdate,
                    Body = new NetworkLatencyUpdateBody
                    {
                        Peer = new NetPeer(peer),
                        Latency = latency
                    }
                }).ContinueWithLogException(_logger, "Failed to handle NetworkLatencyUpdateEvent");
        }

        private void SubscribeOnDeliveryEventEvent()
        {
            _listener.DeliveryEvent += (peer, userData) =>
                _courier.EmitEventAsync(new Package
                {
                    Route = DeliveryEvent,
                    Body = new DeliveryEventBody
                    {
                        Peer = new NetPeer(peer),
                        UserData = userData
                    }
                }).ContinueWithLogException(_logger, "Failed to handle DeliveryEvent");
        }

        private void SubscribeOnNtpResponseEventEvent()
        {
            _listener.NtpResponseEvent += ntpPacket =>
                _courier.EmitEventAsync(new Package
                    {
                        Route = NtpResponseEvent,
                        Body = ntpPacket
                    })
                    .ContinueWithLogException(_logger, "Failed to handle NtpResponseEvent");
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
