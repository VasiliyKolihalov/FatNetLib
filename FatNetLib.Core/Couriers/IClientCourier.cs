using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Wrappers;

namespace Kolyhalov.FatNetLib.Core.Couriers
{
    public interface IClientCourier : ICourier
    {
        public INetPeer ServerPeer { get; }

        public Package? SendToServer(Package package);
    }
}
