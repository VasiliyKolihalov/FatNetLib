using Kolyhalov.FatNetLib.Wrappers;

namespace Kolyhalov.FatNetLib.Subscribers
{
    public interface IConnectionRequestEventSubscriber
    {
        public void Handle(IConnectionRequest connectionRequest);
    }
}
