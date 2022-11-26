using System;
using System.Net;
using Kolyhalov.FatNetLib.Core.Models;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Kolyhalov.FatNetLib.Core.Wrappers
{
    public class NetPeer : INetPeer
    {
        private readonly LiteNetLib.NetPeer _liteLibPeer;

        public NetPeer(LiteNetLib.NetPeer peer)
        {
            _liteLibPeer = peer;
        }

        public int Id => _liteLibPeer.Id;

        public IPEndPoint EndPoint => _liteLibPeer.EndPoint;

        public object? RelatedObject
        {
            get => _liteLibPeer.Tag;
            set => _liteLibPeer.Tag = value;
        }

        public NetStatistics Statistics => _liteLibPeer.Statistics;

        public ConnectionState ConnectionState => _liteLibPeer.ConnectionState;

        public int Ping => _liteLibPeer.Ping;

        public int Mtu => _liteLibPeer.Mtu;

        public long RemoteTimeDelta => _liteLibPeer.RemoteTimeDelta;

        public DateTime RemoteUtcTime => _liteLibPeer.RemoteUtcTime;

        public int TimeSinceLastPacket => _liteLibPeer.TimeSinceLastPacket;

        public void Send(Package package)
        {
            var writer = new NetDataWriter();
            writer.Put(package.Serialized);
            _liteLibPeer.Send(writer, DeliveryMethodConverter.ToFatNetLib(package.Reliability!.Value));
        }
    }
}
