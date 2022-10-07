namespace Kolyhalov.FatNetLib;

public interface IDependencyContext
{
    public void Put(string id, object dependency);

    public void Put<T>(T dependency);

    public void CopyReference(Type from, Type to);

    public T Get<T>(string id);

    public T Get<T>();
}