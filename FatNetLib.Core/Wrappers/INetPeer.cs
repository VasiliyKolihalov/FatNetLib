using System;
using System.Net;
using LiteNetLib;

namespace Kolyhalov.FatNetLib.Core.Wrappers
{
    public interface INetPeer
    {
        public Guid Id { get; }

        public IPEndPoint EndPoint { get; }

        public object? RelatedObject { get; set; }

        public NetStatistics Statistics { get; }

        public ConnectionState ConnectionState { get; }

        public int Ping { get; }

        public int Mtu { get; }

        public long RemoteTimeDelta { get; }

        public DateTime RemoteUtcTime { get; }

        public int TimeSinceLastPacket { get; }
    }
}
