﻿using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Monitors;
using Kolyhalov.FatNetLib.Core.Runners;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Wrappers;

namespace Kolyhalov.FatNetLib.Core
{
    public class ServerCourier : Courier
    {
        public ServerCourier(
            IList<INetPeer> connectedPeers,
            IEndpointsStorage endpointsStorage,
            IResponsePackageMonitor responsePackageMonitor,
            IMiddlewaresRunner sendingMiddlewaresRunner)
            : base(
                connectedPeers,
                endpointsStorage,
                responsePackageMonitor,
                sendingMiddlewaresRunner)
        {
        }

        public void Broadcast(Package package)
        {
            // Todo: make thead-safe
            foreach (INetPeer connectedPeer in ConnectedPeers)
            {
                package.ToPeerId = connectedPeer.Id;
                Send(package);
            }
        }
    }
}
