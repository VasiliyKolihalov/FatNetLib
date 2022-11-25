using System;
using Kolyhalov.FatNetLib.Core.Storages;

namespace Kolyhalov.FatNetLib.Core.Modules.Steps
{
    public class PutScriptStep : IModuleStep
    {
        private readonly Action<IDependencyContext> _script;
        private readonly IDependencyContext _dependencyContext;

        public PutScriptStep(
            string name,
            Action<IDependencyContext> script,
            Type parentModuleType,
            IDependencyContext dependencyContext)
        {
            _script = script;
            Id = new StepId(parentModuleType, GetType(), name);
            _dependencyContext = dependencyContext;
        }

        public StepId Id { get; }

        public void Run()
        {
            _script.Invoke(_dependencyContext);
        }

        public IModuleStep CopyWithNewId(StepId newId)
        {
            return new PutScriptStep((string)newId.Qualifier, _script, newId.ParentModuleType, _dependencyContext);
        }
    }
}
