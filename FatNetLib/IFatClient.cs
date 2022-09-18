namespace Kolyhalov.FatNetLib;

public interface IFatClient
{
    public Package? SendPackage(Package package, int receivingPeerId);
}