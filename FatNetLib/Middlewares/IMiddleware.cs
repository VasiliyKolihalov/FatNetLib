namespace Kolyhalov.FatNetLib.Middlewares
{
    public interface IMiddleware
    {
        public void Process(Package package);
    }
}
