using Kolyhalov.FatNetLib.Core.Models;

namespace Kolyhalov.FatNetLib.Core.Couriers
{
    public interface IServerCourier : ICourier
    {
        public void Broadcast(Package package);

        public void Broadcast(Package package, int ignorePeer);
    }
}
