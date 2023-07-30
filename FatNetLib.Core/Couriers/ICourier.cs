using System.Collections.Generic;
using System.Threading.Tasks;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Wrappers;

namespace Kolyhalov.FatNetLib.Core.Couriers
{
    public interface ICourier
    {
        public IList<INetPeer> Peers { get; }

        public Task<Package?> SendAsync(Package package);

        public Task EmitEventAsync(Package package);
    }
}
