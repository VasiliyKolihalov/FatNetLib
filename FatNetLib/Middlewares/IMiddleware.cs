namespace Kolyhalov.FatNetLib.Middlewares;

public interface IMiddleware
{
    public Package Process(Package package);
}