using LiteNetLib;

namespace Kolyhalov.FatNetLib;

public interface IPackageHandler
{
    public void Handle(Package requestPackage, int peerId, DeliveryMethod deliveryMethod);
}