using Kolyhalov.FatNetLib.LiteNetLibWrappers;

namespace Kolyhalov.FatNetLib.Subscribers;

public interface IConnectionRequestEventSubscriber
{
    public void Handle(IConnectionRequest connectionRequest);
}