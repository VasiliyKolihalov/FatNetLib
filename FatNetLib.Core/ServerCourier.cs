using System.Collections.Generic;
using System.Linq;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Monitors;
using Kolyhalov.FatNetLib.Core.Runners;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Wrappers;

namespace Kolyhalov.FatNetLib.Core
{
    // Todo: make this class thead-safe
    public class ServerCourier : Courier
    {
        public ServerCourier(
            IList<INetPeer> connectedPeers,
            IEndpointsStorage endpointsStorage,
            IResponsePackageMonitor responsePackageMonitor,
            IMiddlewaresRunner sendingMiddlewaresRunner,
            IEndpointsInvoker endpointsInvoker,
            ILogger logger)
            : base(
                connectedPeers,
                endpointsStorage,
                responsePackageMonitor,
                sendingMiddlewaresRunner,
                endpointsInvoker,
                logger)
        {
        }

        public void Broadcast(Package package)
        {
            foreach (INetPeer connectedPeer in ConnectedPeers)
            {
                package.ToPeerId = connectedPeer.Id;
                Send(package);
            }
        }

        public void Broadcast(Package package, int ignorePeer)
        {
            if (ConnectedPeers.All(_ => _.Id != ignorePeer))
                throw new FatNetLibException($"Not found peer to ignore. Specified id {ignorePeer}");

            foreach (INetPeer connectedPeer in ConnectedPeers)
            {
                if (connectedPeer.Id == ignorePeer)
                    continue;

                package.ToPeerId = connectedPeer.Id;
                Send(package);
            }
        }
    }
}
