using System;
using System.Collections.Generic;

namespace Kolyhalov.FatNetLib
{
    public class DependencyContext : IDependencyContext
    {
        private readonly IDictionary<string, Func<IDependencyContext, object>> _dependencyProviders =
            new Dictionary<string, Func<IDependencyContext, object>>();

        private readonly IDictionary<string, object> _dependencies = new Dictionary<string, object>();

        public void Put(string id, Func<IDependencyContext, object> dependencyProvider)
        {
            _dependencyProviders[id] = dependencyProvider;
        }

        public void Put<T>(Func<IDependencyContext, T> dependencyProvider) where T : class
        {
            Put(typeof(T).Name, dependencyProvider);
        }

        public void CopyReference(Type from, Type to)
        {
            Put(to.Name, context => context.Get<object>(from.Name));
        }

        public T Get<T>(string id)
        {
            if (_dependencies.ContainsKey(id))
                return (T)_dependencies[id];

            var dependency = (T)_dependencyProviders[id].Invoke(this);
            _dependencies[id] = dependency!;
            _dependencyProviders.Remove(id);
            return dependency;
        }

        public T Get<T>()
        {
            return Get<T>(typeof(T).Name);
        }
    }
}
