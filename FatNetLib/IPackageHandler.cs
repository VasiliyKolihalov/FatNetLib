using LiteNetLib;

namespace Kolyhalov.FatNetLib;

public interface IPackageHandler
{
    public void InvokeEndpoint(Package requestPackage, int peerId, DeliveryMethod deliveryMethod);
}