namespace Kolyhalov.FatNetLib;

public class DependencyContext : IDependencyContext
{
    private readonly IDictionary<string, object> _dependencies = new Dictionary<string, object>();

    public void Put(string id, object dependency)
    {
        _dependencies[id] = dependency;
    }
    
    public void Put<T>(T dependency)
    {
        Put(typeof(T).Name, dependency!);
    }

    public void CopyReference(Type from, Type to)
    {
        Put(to.Name, Get<object>(from.Name));
    }

    public T Get<T>(string id)
    {
        return (T)_dependencies[id];
    }
    
    public T Get<T>()
    {
        return Get<T>(id: typeof(T).Name);
    }
}