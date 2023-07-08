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
    public class ClientCourier : Courier, IClientCourier
    {
        public ClientCourier(
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

        public INetPeer ServerPeer => ConnectedPeers[0];

        public async Task<Package?> SendToServerAsync(Package package)
        {
            package.Receiver = ServerPeer;
            return await SendAsync(package);
        }
    }
}
