using System.Threading.Tasks;
using Kolyhalov.FatNetLib.Core.Models;

namespace Kolyhalov.FatNetLib.Core.Couriers
{
    public interface IServerCourier : ICourier
    {
        public Task BroadcastAsync(Package package);

        public Task BroadcastAsync(Package package, int ignorePeer);
    }
}
