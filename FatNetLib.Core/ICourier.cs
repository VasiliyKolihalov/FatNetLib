using Kolyhalov.FatNetLib.Core.Models;

namespace Kolyhalov.FatNetLib.Core
{
    public interface ICourier
    {
        public Package? SendPackage(Package package);
    }
}
