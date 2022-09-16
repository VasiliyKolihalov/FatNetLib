namespace Kolyhalov.FatNetLib;

public interface IMiddleware
{
    public Package Process(Package package);
}