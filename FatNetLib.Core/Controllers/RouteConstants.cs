using Kolyhalov.FatNetLib.Core.Microtypes;

namespace Kolyhalov.FatNetLib.Core.Controllers
{
    public static class RouteConstants
    {
        public static class Routes
        {
            public static readonly Route Framework = new Route(Strings.Framework);
            public static readonly Route FrameworkEvents = new Route(Strings.FrameworkEvents);

            public static class Events
            {
                public static readonly Route NetworkReceived = new Route(Strings.Events.NetworkReceived);
                public static readonly Route PeerConnected = new Route(Strings.Events.PeerConnected);
                public static readonly Route PeerDisconnected = new Route(Strings.Events.PeerDisconnected);
                public static readonly Route ConnectionRequest = new Route(Strings.Events.ConnectionRequest);
                public static readonly Route InitializationFinished = new Route(Strings.Events.InitializationFinished);
            }
        }

        public static class Strings
        {
            public const string Framework = "fat-net-lib";
            public const string FrameworkEvents = Framework + "/events";

            public static class Events
            {
                public const string NetworkReceived = FrameworkEvents + "/network-received/handle";
                public const string PeerConnected = FrameworkEvents + "/peer-connected/handle";
                public const string PeerDisconnected = FrameworkEvents + "/peer-disconnected/handle";
                public const string ConnectionRequest = FrameworkEvents + "/connection-request/handle";
                public const string InitializationFinished = FrameworkEvents + "/init-finished/handle";
            }
        }
    }
}
