using System;
using Kolyhalov.FatNetLib.Core.Storages;

namespace Kolyhalov.FatNetLib.Core.Modules.Steps
{
    public class PutDependencyStep : IModuleStep
    {
        private readonly Func<IDependencyContext, object> _dependencyProvider;
        private readonly IDependencyContext _dependencyContext;

        public PutDependencyStep(
            Type parentModuleType,
            string id,
            Func<IDependencyContext, object> dependencyProvider,
            IDependencyContext dependencyContext)
        {
            DependencyId = id;
            Id = new StepId(parentModuleType, GetType(), id);
            _dependencyProvider = dependencyProvider;
            _dependencyContext = dependencyContext;
        }

        public StepId Id { get; }

        private string DependencyId { get; }

        public void Run()
        {
            object dependency = _dependencyProvider.Invoke(_dependencyContext);
            _dependencyContext.Put(DependencyId, dependency);
        }

        public IModuleStep CopyWithNewId(StepId newId)
        {
            return new PutDependencyStep(
                newId.ParentModuleType, (string)newId.InModuleId, _dependencyProvider, _dependencyContext);
        }
    }
}
