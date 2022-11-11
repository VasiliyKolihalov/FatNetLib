namespace Kolyhalov.FatNetLib.Core.Middlewares
{
    public interface IMiddleware
    {
        public void Process(Package package);
    }
}
