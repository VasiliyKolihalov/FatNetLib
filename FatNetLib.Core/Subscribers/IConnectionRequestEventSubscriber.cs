using Kolyhalov.FatNetLib.Core.Wrappers;

namespace Kolyhalov.FatNetLib.Core.Subscribers
{
    public interface IConnectionRequestEventSubscriber
    {
        public void Handle(IConnectionRequest connectionRequest);
    }
}
