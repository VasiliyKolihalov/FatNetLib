using Kolyhalov.FatNetLib.Core.Models;

namespace Kolyhalov.FatNetLib.Core
{
    public interface IEventsEmitter
    {
        public void Emit(Package package);
    }
}
