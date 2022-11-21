namespace Kolyhalov.FatNetLib.Core.Storages
{
    public interface IDependencyContext
    {
        public void Put(string id, object dependency);

        public void Put<T>(T dependency) where T : class;

        public T Get<T>(string id);

        public T Get<T>();
    }
}
