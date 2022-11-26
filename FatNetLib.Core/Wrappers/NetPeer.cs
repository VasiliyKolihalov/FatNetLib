using System;
using System.Net;
using Kolyhalov.FatNetLib.Core.Models;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Kolyhalov.FatNetLib.Core.Wrappers
{
    public class NetPeer : INetPeer
    {
        private readonly LiteNetLib.NetPeer _liteNetLibPeer;

        public NetPeer(LiteNetLib.NetPeer netPeer)
        {
            _liteNetLibPeer = netPeer;
        }

        public int Id => _liteNetLibPeer.Id;

        public IPEndPoint EndPoint => _liteNetLibPeer.EndPoint;

        public object? RelatedObject
        {
            get => _liteNetLibPeer.Tag;
            set => _liteNetLibPeer.Tag = value;
        }

        public NetStatistics Statistics => _liteNetLibPeer.Statistics;

        public ConnectionState ConnectionState => _liteNetLibPeer.ConnectionState;

        public int Ping => _liteNetLibPeer.Ping;

        public int Mtu => _liteNetLibPeer.Mtu;

        public long RemoteTimeDelta => _liteNetLibPeer.RemoteTimeDelta;

        public DateTime RemoteUtcTime => _liteNetLibPeer.RemoteUtcTime;

        public int TimeSinceLastPacket => _liteNetLibPeer.TimeSinceLastPacket;

        public void Send(Package package)
        {
            var writer = new NetDataWriter();
            writer.Put(package.Serialized);
            _liteNetLibPeer.Send(writer, DeliveryMethodConverter.ToFatNetLib(package.Reliability!.Value));
        }
    }
}
