using Kolyhalov.FatNetLib.Wrappers;

namespace Kolyhalov.FatNetLib.Subscribers;

public class ClientConnectionRequestEventSubscriber : IConnectionRequestEventSubscriber
{
    public void Handle(IConnectionRequest request)
    {
        // No actions required
    }
}