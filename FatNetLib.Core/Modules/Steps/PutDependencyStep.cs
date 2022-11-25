using System;
using Kolyhalov.FatNetLib.Core.Storages;

namespace Kolyhalov.FatNetLib.Core.Modules.Steps
{
    public class PutDependencyStep : IModuleStep
    {
        private readonly Func<IDependencyContext, object> _dependencyProvider;
        private readonly IDependencyContext _dependencyContext;
        private readonly string _dependencyId;

        public PutDependencyStep(
            Type parentModuleType,
            string id,
            Func<IDependencyContext, object> dependencyProvider,
            IDependencyContext dependencyContext)
        {
            _dependencyId = id;
            _dependencyProvider = dependencyProvider;
            _dependencyContext = dependencyContext;
            Id = new StepId(parentModuleType, GetType(), id);
        }

        public StepId Id { get; }

        public void Run()
        {
            object dependency = _dependencyProvider.Invoke(_dependencyContext);
            _dependencyContext.Put(_dependencyId, dependency);
        }

        public IModuleStep CopyWithNewId(StepId newId)
        {
            return new PutDependencyStep(
                newId.ParentModuleType, (string)newId.Qualifier, _dependencyProvider, _dependencyContext);
        }
    }
}
