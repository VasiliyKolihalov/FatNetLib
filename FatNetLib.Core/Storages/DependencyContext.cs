using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Utils;

namespace Kolyhalov.FatNetLib.Core.Storages
{
    public class DependencyContext : IDependencyContext
    {
        private readonly IDictionary<string, object> _dependencies = new Dictionary<string, object>();

        public void Put(string id, object dependency)
        {
            _dependencies[id] = dependency;
        }

        public void Put<T>(T dependency) where T : class
        {
            Put(typeof(T).ToDependencyId(), dependency);
        }

        public T Get<T>(string id)
        {
            return (T)_dependencies[id];
        }

        public T Get<T>()
        {
            return Get<T>(typeof(T).ToDependencyId());
        }
    }
}
