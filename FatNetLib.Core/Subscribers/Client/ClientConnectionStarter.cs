using System;
using System.Threading;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Providers;
using Kolyhalov.FatNetLib.Core.Wrappers;
using static LiteNetLib.ConnectionState;

namespace Kolyhalov.FatNetLib.Core.Subscribers.Client
{
    public class ClientConnectionStarter : IConnectionStarter
    {
        private static readonly TimeSpan ConnectionStatePollingPeriod = TimeSpan.FromMilliseconds(50);

        private readonly INetManager _netManager;
        private readonly ClientConfiguration _configuration;
        private readonly string _protocolVersion;

        public ClientConnectionStarter(
            INetManager netManager,
            ClientConfiguration configuration,
            IProtocolVersionProvider protocolVersionProvider)
        {
            _netManager = netManager;
            _configuration = configuration;
            _protocolVersion = protocolVersionProvider.Get();
        }

        public void StartConnection()
        {
            bool started = _netManager.Start();
            if (!started)
                throw new FatNetLibException("Can't start client");

            INetPeer serverPeer = _netManager.Connect(
                                _configuration.Address!,
                                _configuration.Port!.Value,
                                key: _protocolVersion)
                            ?? throw new FatNetLibException("Can't connect client to the server");

            while (serverPeer.ConnectionState == Outgoing)
            {
                Thread.Sleep(ConnectionStatePollingPeriod);
            }

            if (serverPeer.ConnectionState != Connected)
                throw new FatNetLibException("Can't connect client to the server");
        }
    }
}
