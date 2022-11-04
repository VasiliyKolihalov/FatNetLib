namespace Kolyhalov.FatNetLib;

public interface IClient
{
    public Package? SendPackage(Package package);
}
