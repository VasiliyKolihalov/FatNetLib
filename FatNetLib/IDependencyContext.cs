using System;

namespace Kolyhalov.FatNetLib
{
    public interface IDependencyContext
    {
        public void Put(string id, Func<IDependencyContext, object> dependencyProvider);

        public void Put<T>(Func<IDependencyContext, T> dependencyProvider) where T : class;

        public void CopyReference(Type from, Type to);

        public T Get<T>(string id);

        public T Get<T>();
    }
}
