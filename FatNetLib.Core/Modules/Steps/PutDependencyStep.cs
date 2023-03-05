using System;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Storages;

namespace Kolyhalov.FatNetLib.Core.Modules.Steps
{
    public class PutDependencyStep : IStep
    {
        private readonly string _dependencyId;
        private readonly Func<IDependencyContext, object> _dependencyProvider;
        private readonly IDependencyContext _dependencyContext;

        public PutDependencyStep(
            string dependencyId,
            Func<IDependencyContext, object> dependencyProvider,
            IDependencyContext dependencyContext)
        {
            _dependencyId = dependencyId;
            _dependencyProvider = dependencyProvider;
            _dependencyContext = dependencyContext;
        }

        public object Qualifier => _dependencyId;

        public void Run()
        {
            if (_dependencyContext.ContainsKey(_dependencyId))
                throw new FatNetLibException($"Dependency {_dependencyId} was already put");

            _dependencyContext.Put(_dependencyId, _dependencyProvider.Invoke(_dependencyContext));
        }
    }
}
