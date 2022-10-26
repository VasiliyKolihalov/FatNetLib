namespace Kolyhalov.FatNetLib;

public class DependencyContext : IDependencyContext
{
    private readonly IDictionary<string, Func<IDependencyContext, object>> _lazyDependencies =
        new Dictionary<string, Func<IDependencyContext, object>>();

    private readonly IDictionary<string, object> _pinnedDependencies = new Dictionary<string, object>();

    public void Put(string id, Func<IDependencyContext, object> dependency)
    {
        _lazyDependencies[id] = dependency;
    }

    public void Put<T>(Func<IDependencyContext, T> dependency) where T : class
    {
        Put(typeof(T).Name, dependency);
    }

    public void CopyReference(Type from, Type to)
    {
        Put(to.Name, context => context.Get<object>(from.Name));
    }

    public T Get<T>(string id)
    {
        if (_pinnedDependencies.ContainsKey(id))
            return (T)_pinnedDependencies[id];

        var dependency = (T)_lazyDependencies[id].Invoke(this);
        _pinnedDependencies[id] = dependency!;
        _lazyDependencies.Remove(id);
        return dependency;
    }

    public T Get<T>()
    {
        return Get<T>(typeof(T).Name);
    }
}