using Kolyhalov.FatNetLib.Core.Models;

namespace Kolyhalov.FatNetLib.Core
{
    public interface ICourier
    {
        public Package? Send(Package package);

        public void EmitEvent(Package package);
    }
}
