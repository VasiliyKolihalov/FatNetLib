using System.Collections.Generic;
using System.Threading.Tasks;
using Kolyhalov.FatNetLib.Core.Components;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Monitors;
using Kolyhalov.FatNetLib.Core.Runners;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Wrappers;

namespace Kolyhalov.FatNetLib.Core.Couriers
{
    // Todo: make this class thread-safe
    public class ServerCourier : Courier, IServerCourier
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

        public async Task BroadcastAsync(Package package)
        {
            foreach (INetPeer connectedPeer in ConnectedPeers)
            {
                package.Receiver = connectedPeer;
                await SendAsync(package);
            }
        }

        public async Task BroadcastAsync(Package package, int ignorePeer)
        {
            foreach (INetPeer connectedPeer in ConnectedPeers)
            {
                if (connectedPeer.Id == ignorePeer)
                    continue;

                package.Receiver = connectedPeer;
                await SendAsync(package);
            }
        }
    }
}
