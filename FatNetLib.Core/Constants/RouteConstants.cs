using Kolyhalov.FatNetLib.Core.Microtypes;

namespace Kolyhalov.FatNetLib.Core.Constants
{
    public static class RouteConstants
    {
        public static class Routes
        {
            public static readonly Route Framework = new Route(Strings.Framework);
            public static readonly Route FrameworkEvents = new Route(Strings.FrameworkEvents);

            public static class Event
            {
                public static readonly Route NetworkReceived = new Route(Strings.Event.NetworkReceived);
                public static readonly Route PeerConnected = new Route(Strings.Event.PeerConnected);
                public static readonly Route PeerDisconnected = new Route(Strings.Event.PeerDisconnected);
                public static readonly Route ConnectionRequest = new Route(Strings.Event.ConnectionRequest);
                public static readonly Route NetworkError = new Route(Strings.Event.NetworkError);

                public static readonly Route NetworkReceiveUnconnected =
                    new Route(Strings.Event.NetworkReceiveUnconnected);

                public static readonly Route NetworkLatencyUpdate =
                    new Route(Strings.Event.NetworkLatencyUpdate);

                public static readonly Route DeliveryEvent = new Route(Strings.Event.DeliveryEvent);
                public static readonly Route NtpResponseEvent = new Route(Strings.Event.NtpResponseEvent);

                public static readonly Route InitializationFinished =
                    new Route(Strings.Event.InitializationFinished);
            }
        }

        public static class Strings
        {
            public const string Framework = "fat-net-lib";
            public const string FrameworkEvents = Framework + "/events";

            public static class Event
            {
                public const string NetworkReceived = FrameworkEvents + "/network-received/handle";
                public const string PeerConnected = FrameworkEvents + "/peer-connected/handle";
                public const string PeerDisconnected = FrameworkEvents + "/peer-disconnected/handle";
                public const string ConnectionRequest = FrameworkEvents + "/connection-request/handle";
                public const string NetworkError = FrameworkEvents + "/network-error/handle";
                public const string NetworkReceiveUnconnected = FrameworkEvents + "/network-receive-unconnected/handle";
                public const string NetworkLatencyUpdate = FrameworkEvents + "/network-latency-update/handle";
                public const string DeliveryEvent = FrameworkEvents + "/delivery-event/handle";
                public const string NtpResponseEvent = FrameworkEvents + "/ntp-response/handle";
                public const string InitializationFinished = FrameworkEvents + "/init-finished/handle";
            }
        }
    }
}
