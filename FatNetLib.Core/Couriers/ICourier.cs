using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Wrappers;

namespace Kolyhalov.FatNetLib.Core.Couriers
{
    public interface ICourier
    {
        public IList<INetPeer> Peers { get; }

        public Package? Send(Package package);

        public void EmitEvent(Package package);
    }
}
