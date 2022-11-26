﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Runners;
using Kolyhalov.FatNetLib.Core.Wrappers;
using static Kolyhalov.FatNetLib.Core.Utils.ExceptionUtils;

namespace Kolyhalov.FatNetLib.Core.Subscribers.Client
{
    public class ClientPeerConnectedEventSubscriber : IPeerConnectedEventSubscriber
    {
        private readonly IList<INetPeer> _connectedPeers;
        private readonly IInitializersRunner _initializersRunner;
        private readonly ILogger _logger;

        public ClientPeerConnectedEventSubscriber(
            IList<INetPeer> connectedPeers,
            IInitializersRunner initializersRunner,
            ILogger logger)
        {
            _connectedPeers = connectedPeers;
            _initializersRunner = initializersRunner;
            _logger = logger;
        }

        public void Handle(INetPeer peer)
        {
            _connectedPeers.Add(peer);

            Task.Run(() =>
                CatchExceptionsTo(_logger, @try: () =>
                    _initializersRunner.Run()));
        }
    }
}
