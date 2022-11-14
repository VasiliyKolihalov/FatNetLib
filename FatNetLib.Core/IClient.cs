namespace Kolyhalov.FatNetLib.Core
{
    public interface IClient
    {
        public Package? SendPackage(Package package);
    }
}
